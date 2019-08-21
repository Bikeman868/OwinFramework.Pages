exported.store = function () {
    var newStore = function (store) {
        if (store.recordType == undefined) throw "Data stores must have a recordType";
        if (store.fields == undefined) throw "Data stores must define some fields";
        store.list = null;
        store.dictionary = {};
        store.dispatcherUnsubscribe = null;
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

        if (store.copyObject == undefined) {
            store.copyObject = function (from, to) {
                to = to || {};
                store.fields.forEach(function (field) {
                    var value = from[field.name];
                    if (value == undefined) return;
                    if (field.type === Object) {
                        to[field.name] = Object.assign({}, value);
                    } else if (field.type === Array) {
                        var a = [];
                        for (let i = 0; i < value.length; i++) {
                            if (typeof (value[i]) === "object")
                                a.push(Object.assign({}, value[i]));
                            else
                                a.push(value[i]);
                        }
                        to[field.name] = a;
                    } else {
                        to[field.name] = value;
                    }
                });
                return to;
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
                                for (let i = 0; i < modifiedValue.length; i++) {
                                    if (typeof(modifiedValue[i]) === "object") {
                                        var hasChanges = false;
                                        for (p in modifiedValue[i]) {
                                            if (originalValue[i][p] != modifiedValue[i][p]) {
                                                hasChanges = true;
                                            }
                                        }
                                        if (hasChanges) changes.push({ name: field.name, index: i, objectValue: modifiedValue[i] });
                                    } else {
                                        if (originalValue[i] != modifiedValue[i])
                                            changes.push({ name: field.name, index: i, value: modifiedValue[i] });
                                    }
                                }
                            }
                        }
                        else if (originalValue != modifiedValue)
                            changes.push({ name: field.name, value: modifiedValue });
                    });
                }
                return changes;
            }
        }

        if (store.dispose == undefined) {
            store.dispose = function () {
                if (store.dispatcherUnsubscribe != null) store.dispatcherUnsubscribe();
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
                    if (records[i] != undefined && records[i][store.keyField] != undefined)
                        store.dictionary[records[i][store.keyField]] = records[i];
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
                    store.add(record);
                } else {
                    store.copyObject(record, original);
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
                        } else if (response[store.idField] == undefined) {
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

        if (store.cloneForEditing == undefined) {
            store.cloneForEditing = function (original) {
                return store.copyObject(original);
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
                return record;
            }
        }

        if (store.recordType != undefined && store.dispatcher != undefined) {
            store.dispatcherUnsubscribe = store.dispatcher.subscribe(function (message) {
                if (message.propertyChanges != undefined) {
                    for (let i = 0; i < message.propertyChanges.length; i++) {
                        var propertyChange = message.propertyChanges[i];
                        if (propertyChange.recordType === store.recordType) {
                            var record = store.get(propertyChange.id);
                            if (record != undefined) {
                                record[propertyChange.name] = propertyChange.value;
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
                if (message.childListChanges != undefined) {
                    for (let i = 0; i < message.childListChanges.length; i++) {
                        var childListChange = message.childListChanges[i];
                        if (childListChange.recordType === store.recordType) {
                            // TODO: refresh child list
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
