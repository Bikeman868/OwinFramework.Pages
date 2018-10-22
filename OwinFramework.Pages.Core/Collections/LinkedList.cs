using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Exceptions;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// This is a high performance thread-safe linked list whose
    /// design makes it possible to have the same objects exist on
    /// multiple lists (so that the objects can be traversed in
    /// different orders).
    /// Also provides PopFirst and PopLast that enables the list
    /// to be used as a queue or stack.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the list</typeparam>
    public class LinkedList<T> : IEnumerable<T>
    {
        private readonly object _lock = new object();
        private ListElement _head;
        private ListElement _tail;

        /// <summary>
        /// Adds a new element to the end of the list
        /// </summary>
        /// <param name="data">The data to add to the list</param>
        /// <returns>The new list element. This can be used to delete
        /// the element later, or enumerate the list starting from
        /// this element</returns>
        public ListElement Append(T data)
        {
            var listElement = new ListElement
            {
                Data = data
            };

            lock (_lock)
            {
                if (_tail == null)
                {
                    _tail = listElement;
                    _head = listElement;
                }
                else
                {
                    listElement.Prior = _tail;
                    _tail.Next = listElement;
                    _tail = listElement;
                }
            }

            return listElement;
        }

        /// <summary>
        /// Adds a new element to the start of the list
        /// </summary>
        /// <param name="data">The data to add to the list</param>
        /// <returns>The new list element. This can be used to delete
        /// the element later, or enumerate the list starting from
        /// this element</returns>
        public ListElement Prepend(T data)
        {
            var listElement = new ListElement
            {
                Data = data
            };

            lock (_lock)
            {
                if (_head == null)
                {
                    _tail = listElement;
                    _head = listElement;
                }
                else
                {
                    listElement.Next = _head;
                    _head.Prior = listElement;
                    _head = listElement;
                }
            }

            return listElement;
        }

        /// <summary>
        /// Removes an element from anywhere in the list
        /// </summary>
        /// <param name="element">The element to remove</param>
        public void Delete(ListElement element)
        {
            lock (_lock)
            {
                if (element.Prior == null)
                    _head = element.Next;
                else
                    element.Prior.Next = element.Next;

                if (element.Next == null)
                    _tail = element.Prior;
                else
                    element.Next.Prior = element.Prior;
            }
        }

        /// <summary>
        /// Deletes elements from the list that match the supplied predicate
        /// </summary>
        /// <param name="predicate">Deletes elements that are true for this expression</param>
        public void DeleteWhere(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate(current.Data))
                        Delete(current);
                    current = current.Next;
                }
            }
        }

        /// <summary>
        /// Returns true if the list is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock (_lock) return _head == null;
            }
        }

        /// <summary>
        /// Constructs a list of elements that match the supplied predicate
        /// </summary>
        /// <param name="predicate">Returns elements that are true for this expression</param>
        /// <returns>A list of list elements. You can pass these elements to the 
        /// Delete method to remove them from the list</returns>
        public List<ListElement> ToList(Func<T, bool> predicate)
        {
            var result = new List<ListElement>();

            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate(current.Data))
                        result.Add(current);
                    current = current.Next;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the first list element that matches the supplied predicate function.
        /// Returns null if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The first list element that matches the predicate</returns>
        public ListElement FirstOrDefault(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate(current.Data))
                        return current;
                    current = current.Next;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first list element that matches the supplied predicate function.
        /// Throws an exception if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The first list element that matches the predicate</returns>
        public ListElement First(Func<T, bool> predicate)
        {
            var element = FirstOrDefault(predicate);
            if (element == null)
                throw new NotFoundException("No list element matches the supplied predicate");
            return element;
        }

        /// <summary>
        /// Returns the last list element that matches the supplied predicate function.
        /// Returns null if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The last list element that matches the predicate</returns>
        public ListElement LastOrDefault(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                var current = _tail;
                while (current != null)
                {
                    if (predicate(current.Data))
                        return current;
                    current = current.Prior;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the last list element that matches the supplied predicate function.
        /// Throws an exception if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The last list element that matches the predicate</returns>
        public ListElement Last(Func<T, bool> predicate)
        {
            var element = LastOrDefault(predicate);
            if (element == null)
                throw new NotFoundException("No list element matches the supplied predicate");
            return element;
        }

        /// <summary>
        /// Removes the first item from the list and returnes it in a thread-safe way
        /// ensuring that each thread will pop a different item from the list.
        /// </summary>
        public T PopFirst()
        {
            lock (_lock)
            {
                var result = _head;
                if (result == null) return default(T);

                _head = result.Next;
                if (_head == null)
                    _tail = null;
                else
                    _head.Prior = null;

                return result.Data;
            }
        }

        /// <summary>
        /// Removes the last item from the list and returnes it in a thread-safe way
        /// ensuring that each thread will pop a different item from the list.
        /// </summary>
        public T PopLast()
        {
            lock (_lock)
            {
                var result = _tail;
                if (result == null) return default(T);

                _tail = result.Prior;
                if (_tail == null)
                    _head = null;
                else
                    _tail.Next = null;

                return result.Data;
            }
        }

        /// <summary>
        /// Implements IEnumerable so that you can use Linq expressions with the list
        /// The enumerator is thread-safe and can be reset to start back at the beginning
        /// of the list
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new ForwardEnumerator(this, null);
        }

        /// <summary>
        /// Implements IEnumerable
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerates the rest of the list starting at the specified list element.
        /// The starting element will the Current value of the enumerator to begin
        /// with and the first call to MoveNext() will advance to the next item in
        /// the list
        /// </summary>
        /// <param name="start">Where to start. Pass null to start from the 
        /// beginning/end of the list</param>
        /// <param name="forwards">Choose whether to go forwards or backwards in
        /// the list</param>
        /// <returns>A thread-safe enumerator</returns>
        public IEnumerator<T> EnumerateFrom(ListElement start, bool forwards = true)
        {
            return forwards
                ? (IEnumerator<T>)new ForwardEnumerator(this, start)
                : new ReverseEnumerator(this, start);
        }

        private class ForwardEnumerator : IEnumerator<T>
        {
            private readonly LinkedList<T> _list;
            private ListElement _current;

            public ForwardEnumerator(LinkedList<T> list, ListElement start)
            {
                _list = list;
                _current = start;
            }

            public T Current
            {
                get { return _current == null ? default(T) : _current.Data; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_current == null)
                    _current = _list._head;
                else
                {
                    lock (_list._lock)
                    {
                        if (_current.Next == null)
                            return false;
                        _current = _current.Next;
                    }
                }
                return _current != null;
            }

            public void Reset()
            {
                _current = null;
            }
        }

        private class ReverseEnumerator : IEnumerator<T>
        {
            private readonly LinkedList<T> _list;
            private ListElement _current;

            public ReverseEnumerator(LinkedList<T> list, ListElement start)
            {
                _list = list;
                _current = start;
            }

            public T Current
            {
                get { return _current == null ? default(T) : _current.Data; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_current == null)
                    _current = _list._tail;
                else
                {
                    lock (_list._lock)
                    {
                        if (_current.Prior == null)
                            return false;
                        _current = _current.Prior;
                    }
                }
                return _current != null;
            }

            public void Reset()
            {
                _current = null;
            }
        }

        /// <summary>
        /// Wrapper for an element in the linked list
        /// </summary>
        public class ListElement
        {
            /// <summary>
            /// A reference to the linked list data
            /// </summary>
            public T Data;

            /// <summary>
            /// A reference to the next item in the list
            /// </summary>
            public ListElement Next;

            /// <summary>
            /// A reference to the prior item in the list
            /// </summary>
            public ListElement Prior;
        }

    }
}