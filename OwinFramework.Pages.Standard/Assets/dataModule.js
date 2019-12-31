exported.store = function () {
    // A data store maintains a client-side copy of data retrieved from the
    // server and optionally subscribes to update messages to keep it in sync
    // with other browser tabs - possibly on other computers. It also provides
    // methods for modifying this data on the client and sending changes back to
    // the server, including lists of child records.
    //
    // To create a store, construct an object to use as your data store and implement
    // the methods that you want to be custom, then pass your object to the newStore
    // function to fill in the rest of the methods with default implementations. Your
    // store must implement at least a fields array and a recordType property. To 
    // read/write data to a back-end service you must implement the crud property.
    // For examples of this see the dataModule.js file in the CMS Manager project.
    //
    // To use the store call these methods of the store:
    //   createRecord - creates a new record, adds it to the data store and writes it to the server
    //   retrieveAllRecords - returns an array of records. You can bind this to a view and new records will be shown in the view
    //   retrieveRecord - retrieves one record by its ID. Gets if from the server and caches on the client
    //   updateRecord - updates a record and sends only the changes back to the server
    //   deleteRecord - deletes the record from the server then removes it from the client-side cache
    //   cloneRecord - makes a copy of an existing record as the basis of a new record. Deep copies the children.
    //   blankRecord - returns a new record with default values for each field
    var newStore = function (store) {
        if (store.recordType == undefined) throw "Data stores must have a recordType";
        if (store.fields == undefined) throw "Data stores must define some fields";
        store.list = null;
        store.dictionary = {};
        store.updateNotifierUnsubscribe = null;
        store.crud = store.crud || {};
        store.name = store.name || store.recordType;
        store.listName = store.listName || "list of " + name;

        var defaultRecord = {};
        store.fields.forEach(function (field) {
            if (field.name == undefined) throw "Data store fields must have a name";
            if (typeof (field.default) === "function")
                defaultRecord[field.name] = field.default(defaultRecord);
            else
                defaultRecord[field.name] = field.default;
        });

        store.fields.forEach(function (field) {
            field.allowNull = field.allowNull == true;
            if (field.type == undefined) {
                var defaultValue = defaultRecord[field.name];
                if (defaultValue == undefined) field.type = String;
                else if (typeof (defaultValue) === "number") field.type = Number;
                else if (typeof (defaultValue) === "bigint") field.type = Number;
                else if (typeof (defaultValue) === "boolean") field.type = Boolean;
                else if (typeof (defaultValue) === "object") field.type = defaultValue.constructor;
                else field.type = String;
            }
            if (field.default == undefined) {
                if (field.type === Number) field.default = 0;
                else if (field.type === Boolean) field.default = false;
                else if (field.type === Array) field.default = function (r) { return []; };
                else if (field.type === Object) field.default = function (r) { return {}; };
                else if (field.type === Date) field.default = function (r) { return new Date(); };
                else field.default = "";
            }
            if (store.keyField == undefined && field.isKey)
                store.keyField = field.name;
        });
        if (store.keyField == undefined) throw "Data stores must define a key field";

        if (store.handleGetSuccess == undefined) {
            store.handleGetSuccess = function (description, response, onSuccess, onFail) {
                if (response == undefined) {
                    if (onFail != undefined) onFail("No " + description + " was returned by the server");
                } else {
                    if (onSuccess != undefined) onSuccess(response);
                }
            }
        }

        if (store.handleGetFail == undefined) {
            store.handleGetFail = function (description, ajax, onFail) {
                if (onFail != undefined) {
                    onFail("Failed to retrieve a " + description);
                }
            }
        }

        if (store.deepCopy == undefined) {
            store.deepCopy = function (value) {
                if (value == undefined) return null;
                if (typeof (value) === "object") {
                    if (value.constructor === Array) {
                        var result = [];
                        value.forEach(function (e) { result.push(store.deepCopy(e)); });
                        return result;
                    } else if (value.constructor === Date) {
                        return new Date(value.valueOf());
                    } else {
                        var result = {};
                        for (p in value) result[p] = store.deepCopy(value[p]);
                        return result;
                    }
                }
                return value;
            }
        }

        if (store.copyRecord == undefined) {
            store.copyRecord = function (from, to) {
                to = to || {};
                store.fields.forEach(function (field) {
                    var value = from[field.name];
                    if (value == undefined) return;
                    to[field.name] = store.deepCopy(value);
                });
                return to;
            }
        }

        if (store.initRecord == undefined) {
            store.initRecord = function (record) {
                store.fields.forEach(function (field) {
                    if (!field.allowNull && record[field.name] == undefined) {
                        if (field.type === Object) record[field.name] = {};
                        else if (field.type === Array) record[field.name] = [];
                        else if (field.type === Date) record[field.name] = new Date();
                        else if (field.type === Number) record[field.name] = 0;
                        else if (field.type === Boolean) record[field.name] = false;
                        else record[field.name] = "";
                    }
                });
            }
        }

        if (store.findChanges == undefined) {
            store.findChanges = function (original, modified) {
                var changes = [];
                if (original == undefined) {
                    store.fields.forEach(function (field) {
                        var value = modified[field.name];
                        if (value != undefined) {
                            if (field.type === Object)
                                changes.push({ name: field.name, objectValue: value });
                            else if (field.type === Array)
                                changes.push({ name: field.name, arrayValue: value });
                            else
                                changes.push({ name: field.name, value: value });
                        }
                    });
                } else {
                    store.fields.forEach(function (field) {
                        var modifiedValue = modified[field.name];
                        var originalValue = original[field.name];
                        if (field.type === Object) {
                            if (modifiedValue == undefined) {
                                if (originalValue != undefined)
                                    changes.push({ name: field.name, objectValue: null });
                            } else if (originalValue == undefined) {
                                changes.push({ name: field.name, objectValue: modifiedValue });
                            } else {
                                var hasChanges = false;
                                var changedProperties = {};
                                for (p in modifiedValue) {
                                    if (originalValue[p] != modifiedValue[p]) {
                                        hasChanges = true;
                                        changedProperties[p] = modifiedValue[p];
                                    }
                                }
                                if (hasChanges)
                                    changes.push({ name: field.name, objectProperties: changedProperties });
                            }
                        }
                        else if (field.type === Array) {
                            if (modifiedValue == undefined) {
                                if (originalValue != undefined && originalValue.length > 0)
                                    changes.push({ name: field.name, arrayValue: null });
                            } else if (originalValue == undefined || originalValue.length != modifiedValue.length) {
                                changes.push({ name: field.name, arrayValue: modifiedValue });
                            } else {
                                for (let i = 0; i < originalValue.length; i++) {
                                    if (typeof (originalValue[i]) === "object") {
                                        var hasChanges = false;
                                        for (p in originalValue[i]) {
                                            if (originalValue[i][p] != modifiedValue[i][p]) {
                                                hasChanges = true;
                                            }
                                        }
                                        if (hasChanges) changes.push({
                                            name: field.name,
                                            index: i,
                                            objectValue: modifiedValue[i]
                                        });
                                    } else {
                                        if (originalValue[i] != modifiedValue[i])
                                            changes.push({
                                                name: field.name,
                                                index: i,
                                                value: modifiedValue[i]
                                            });
                                    }
                                }
                            }
                        }
                        else if (originalValue != modifiedValue)
                            changes.push({
                                name: field.name,
                                value: modifiedValue
                            });
                    });
                }
                return changes;
            }
        }

        if (store.dispose == undefined) {
            store.dispose = function () {
                if (store.updateNotifierUnsubscribe != null) store.updateNotifierUnsubscribe();
            };
        }

        if (store.add == undefined) {
            store.add = function (record) {
                if (store.list != undefined) store.list.push(record);
                store.dictionary[record[store.keyField]] = record;
            };
        }

        if (store.remove == undefined) {
            store.remove = function (id) {
                delete store.dictionary[id];
                if (store.list != undefined) {
                    var index = -1;
                    store.list.forEach(function (e, i) { if (e != undefined && e[store.keyField] === id) index = i; });
                    if (index >= 0) store.list.splice(index, 1);
                }
            };
        }

        if (store.setAll == undefined) {
            store.setAll = function (records) {
                if (records == null) return;
                store.list = records;
                store.dictionary = {};
                for (let i = 0; i < records.length; i++) {
                    var record = records[i];
                    if (record != undefined) {
                        store.initRecord(record);
                        if (record[store.keyField] != undefined)
                            store.dictionary[record[store.keyField]] = record;
                    }
                }
            };
        }

        if (store.getAll == undefined) {
            store.getAll = function () { return store.list; };
        }

        if (store.set == undefined) {
            store.set = function (record) {
                if (record == undefined) return;

                var id = record[store.keyField];
                if (id == undefined) return;

                var original = store.dictionary[id];
                if (original == undefined) {
                    store.initRecord(record);
                    store.add(record);
                } else {
                    store.copyRecord(record, original);
                }
            };
        }

        if (store.get == undefined) {
            store.get = function (id) { return store.dictionary[id]; }
        }

        if (store.createRecord == undefined) {
            store.createRecord = function (newRecord, onSuccess, onFail, params) {
                if (store.crud.createRecord == undefined) return;
                store.crud.createRecord.call(store,
                    newRecord,
                    function (response) {
                        if (response == undefined) {
                            if (onFail != undefined) onFail("No response was received from the server, the " + store.name + " might not have been created");
                        } else if (response[store.keyField] == undefined) {
                            if (onFail != undefined) onFail("The server failed to return an ID for the new " + store.name);
                        } else {
                            store.add(response);
                            if (onSuccess != undefined) onSuccess(response);
                        }
                    },
                    function (ajax) {
                        if (ajax.status === 403) onFail("You do not have permission to create " + store.name);
                        if (onFail != undefined) onFail("Failed to create a new " + store.name);
                    },
                    params);
            }
        }

        if (store.retrieveAllRecords == undefined) {
            store.retrieveAllRecords = function (onSuccess, onFail) {
                if (store.crud.retrieveAllRecords == undefined) return;
                var records = store.getAll();
                if (records == undefined) {
                    store.crud.retrieveAllRecords.call(store,
                        function (response) {
                            if (response == undefined) {
                                if (onFail != undefined) onFail("No " + store.listName + " was returned by the server");
                            } else {
                                store.setAll(response);
                                if (onSuccess != undefined) onSuccess(response);
                            }
                        },
                        function (ajax) {
                            if (ajax.status === 403) onFail("You do not have permission to retrieve " + store.listName);
                            if (onFail != undefined) onFail("Failed to retrieve a " + store.listName);
                        });
                } else {
                    if (onSuccess != undefined) onSuccess(records);
                }
            }
        }

        if (store.retrieveRecord == undefined) {
            store.retrieveRecord = function (id, onSuccess, onFail) {
                if (store.crud.retrieveRecord == undefined) return;
                if (id == undefined) return;
                var record = store.get(id);
                if (record == undefined) {
                    store.crud.retrieveRecord.call(store,
                        id,
                        function (response) {
                            store.set(response);
                            if (onSuccess != undefined) onSuccess(response);
                        },
                        function (ajax) {
                            if (ajax.status === 403) onFail("You do not have permission to retrieve " + store.name + " " + id);
                            if (onFail != undefined) onFail("Failed to retrieve " + store.name + " " + id);
                        });
                } else {
                    if (onSuccess != undefined) onSuccess(record);
                }
            }
        }

        if (store.updateRecord == undefined) {
            store.updateRecord = function (originalRecord, updatedRecord, onSuccess, onFail) {
                if (store.crud.updateRecord == undefined) return;
                var changes = store.findChanges(originalRecord, updatedRecord);
                if (changes.length === 0) {
                    if (onSuccess != undefined) onSuccess(originalRecord);
                } else {
                    var id = originalRecord[store.keyField];
                    if (updatedRecord[store.keyField] !== id) {
                        if (onFail != undefined) {
                            onFail("Original and updated " + store.name + " must have the same id");
                        }
                        return;
                    }
                    store.crud.updateRecord.call(store,
                        originalRecord,
                        updatedRecord,
                        changes,
                        function (response) {
                            if (response == undefined) {
                                if (onFail != undefined) onFail("No response was received from the server, the " + store.name + " might not have been updated");
                            } else {
                                store.set(response);
                                if (onSuccess != undefined) onSuccess(response);
                            }
                        },
                        function (ajax) {
                            if (onFail != undefined) {
                                if (ajax.status === 403) onFail("You do not have permission to update " + store.name);
                                else onFail("Failed to update " + store.name);
                            }
                        });
                }
            }
        }

        if (store.deleteRecord == undefined) {
            store.deleteRecord = function (id, onSuccess, onFail) {
                if (store.crud.deleteRecord == undefined) return;
                store.crud.deleteRecord(
                    id,
                    function (response) {
                        store.remove(id);
                        if (onSuccess != undefined) onSuccess(response);
                    },
                    function (ajax) {
                        if (onFail != undefined) {
                            if (ajax.status === 403) onFail("You do not have permission to delete " + store.name + " " + id);
                            else onFail("Failed to delete " + store.name + " " + id);
                        }
                    });
            }
        }

        if (store.cloneRecord == undefined) {
            store.cloneRecord = function (original) {
                var copy = store.copyRecord(original);
                store.initRecord(copy);
                return copy;
            }
        }

        if (store.blankRecord == undefined) {
            store.blankRecord = function () {
                var record = {};
                store.fields.forEach(function (field) {
                    if (typeof (field.default) === "function")
                        record[field.name] = field.default(record);
                    else
                        record[field.name] = field.default;
                });
                store.initRecord(record);
                return record;
            }
        }

        if (store.recordType != undefined && store.updateNotifier != undefined) {
            store.updateNotifierUnsubscribe = store.updateNotifier.subscribe(function (message) {
                if (message.propertyChanges != undefined) {
                    for (let i = 0; i < message.propertyChanges.length; i++) {
                        var propertyChange = message.propertyChanges[i];
                        if (propertyChange.recordType === store.recordType) {
                            var record = store.get(propertyChange.id);
                            if (record != undefined) {
                                if (propertyChange.index != undefined) {
                                    if (record[propertyChange.name] == undefined)
                                        record[propertyChange.name] = [];
                                    while (record[propertyChange.name].length < propertyChange.index)
                                        record[propertyChange.name].push(null);
                                    if (propertyChange.objectValue != undefined)
                                        record[propertyChange.name][propertyChange.index] = propertyChange.objectValue;
                                    else if (propertyChange.arrayValue != undefined)
                                        record[propertyChange.name][propertyChange.index] = propertyChange.arrayValue;
                                    else
                                        record[propertyChange.name][propertyChange.index] = propertyChange.value;
                                } else {
                                    if (propertyChange.objectValue != undefined)
                                        record[propertyChange.name] = propertyChange.objectValue;
                                    else if (propertyChange.arrayValue != undefined)
                                        record[propertyChange.name] = propertyChange.arrayValue;
                                    else
                                        record[propertyChange.name] = propertyChange.value;
                                }
                            }
                        }
                    }
                }
                if (message.deletedRecords != undefined) {
                    for (let i = 0; i < message.deletedRecords.length; i++) {
                        var deletedElement = message.deletedRecords[i];
                        if (deletedElement.recordType === store.recordType) {
                            store.remove(deletedElement.id);
                        }
                    }
                }
                if (message.newRecords != undefined && store.crud.retrieveRecord != undefined) {
                    for (let i = 0; i < message.newRecords.length; i++) {
                        var newRecord = message.newRecords[i];
                        if (newRecord.recordType === store.recordType) {
                            store.crud.retrieveRecord.call(store, newRecord.id, function (r) { store.set(r); });
                        }
                    }
                }
            });
        }

        return store;
    }

    return {
        newStore: newStore // Creates a new data store
    }
}();
