using System;
using System.Collections.Generic;
using PROG7312_POE.Models;

namespace PROG7312_POE.DataStructures
{
    /// <summary>
    /// A min-heap implementation for managing service requests based on priority.
    /// Lower priority values (higher importance) are processed first.
    /// </summary>
    public class ServiceRequestHeap
    {
        private readonly List<ServiceRequest> _heap;
        private readonly Dictionary<string, int> _requestIndices; // For O(1) lookup

        public int Count => _heap.Count;
        public bool IsEmpty => _heap.Count == 0;

        public ServiceRequestHeap()
        {
            _heap = new List<ServiceRequest>();
            _requestIndices = new Dictionary<string, int>();
        }

        /// <summary>
        /// Adds a new service request to the heap
        /// </summary>
        public void Enqueue(ServiceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (Contains(request.RequestId))
                throw new InvalidOperationException($"Request with ID {request.RequestId} already exists in the heap");

            _heap.Add(request);
            int index = _heap.Count - 1;
            _requestIndices[request.RequestId] = index;
            SiftUp(index);
        }

        /// <summary>
        /// Returns the highest priority request without removing it
        /// </summary>
        public ServiceRequest Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Heap is empty");

            return _heap[0];
        }

        /// <summary>
        /// Removes and returns the highest priority request
        /// </summary>
        public ServiceRequest Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Heap is empty");

            var root = _heap[0];
            _requestIndices.Remove(root.RequestId);

            if (_heap.Count == 1)
            {
                _heap.Clear();
                return root;
            }

            // Move last element to root
            var last = _heap[_heap.Count - 1];
            _heap[0] = last;
            _requestIndices[last.RequestId] = 0;
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
                SiftDown(0);

            return root;
        }

        /// <summary>
        /// Checks if a request with the given ID exists in the heap
        /// </summary>
        public bool Contains(string requestId) => _requestIndices.ContainsKey(requestId);

        /// <summary>
        /// Attempts to get a request by its ID
        /// </summary>
        public bool TryGetRequest(string requestId, out ServiceRequest request)
        {
            if (_requestIndices.TryGetValue(requestId, out int index) && index >= 0 && index < _heap.Count)
            {
                request = _heap[index];
                return true;
            }
            request = null;
            return false;
        }

        /// <summary>
        /// Updates the priority of an existing request
        /// </summary>
        public void UpdatePriority(string requestId, RequestPriority newPriority)
        {
            if (!_requestIndices.TryGetValue(requestId, out int index))
                throw new KeyNotFoundException($"Request with ID {requestId} not found in heap");

            var oldPriority = _heap[index].Priority;
            _heap[index].Priority = newPriority;

            if (newPriority < oldPriority) // Higher priority
                SiftUp(index);
            else if (newPriority > oldPriority) // Lower priority
                SiftDown(index);
        }

        /// <summary>
        /// Removes a specific request from the heap
        /// </summary>
        public bool Remove(string requestId)
        {
            if (!_requestIndices.TryGetValue(requestId, out int index))
                return false;

            // Move the last element to the current position
            _requestIndices.Remove(requestId);
            
            if (index == _heap.Count - 1)
            {
                _heap.RemoveAt(index);
                return true;
            }

            var last = _heap[_heap.Count - 1];
            _heap[index] = last;
            _requestIndices[last.RequestId] = index;
            _heap.RemoveAt(_heap.Count - 1);

            // Restore heap property
            if (index > 0 && Compare(_heap[index], _heap[(index - 1) / 2]) < 0)
                SiftUp(index);
            else
                SiftDown(index);

            return true;
        }

        /// <summary>
        /// Returns all requests in the heap (not in any particular order)
        /// </summary>
        public IEnumerable<ServiceRequest> GetAllRequests() => new List<ServiceRequest>(_heap);

        /// <summary>
        /// Clears all requests from the heap
        /// </summary>
        public void Clear()
        {
            _heap.Clear();
            _requestIndices.Clear();
        }

        #region Private Helper Methods

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (Compare(_heap[index], _heap[parentIndex]) >= 0)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if (leftChild < _heap.Count && Compare(_heap[leftChild], _heap[smallest]) < 0)
                    smallest = leftChild;

                if (rightChild < _heap.Count && Compare(_heap[rightChild], _heap[smallest]) < 0)
                    smallest = rightChild;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            if (i != j)
            {
                (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
                _requestIndices[_heap[i].RequestId] = i;
                _requestIndices[_heap[j].RequestId] = j;
            }
        }

        private static int Compare(ServiceRequest a, ServiceRequest b)
        {
            // First compare by priority (lower enum value = higher priority)
            int priorityComparison = a.Priority.CompareTo(b.Priority);
            if (priorityComparison != 0)
                return priorityComparison;

            // If priorities are equal, older requests come first
            return a.DateSubmitted.CompareTo(b.DateSubmitted);
        }

        #endregion
    }
}
