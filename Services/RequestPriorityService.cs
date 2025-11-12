using System;
using System.Collections.Generic;
using System.Linq;
using PROG7312_POE.DataStructures;
using PROG7312_POE.Models;

namespace PROG7312_POE.Services
{
    /// <summary>
    /// Service for managing service request priorities and processing order
    /// </summary>
    public class RequestPriorityService : IDisposable
    {
        private readonly ServiceRequestHeap _priorityQueue;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public RequestPriorityService()
        {
            _priorityQueue = new ServiceRequestHeap();
            
            // Add some sample data for testing
            AddSampleData();
        }
        
        private void AddSampleData()
        {
            // Only add sample data if the queue is empty
            if (_priorityQueue.IsEmpty)
            {
                var sampleRequests = new List<ServiceRequest>
                {
                    new ServiceRequest
                    {
                        RequestId = "REQ-1001",
                        Title = "Pothole on Main Street",
                        Description = "Large pothole causing traffic issues",
                        Priority = RequestPriority.High,
                        Status = RequestStatus.Submitted,
                        DateSubmitted = DateTime.Now.AddHours(-2),
                        Location = "Main Street, Downtown"
                    },
                    new ServiceRequest
                    {
                        RequestId = "REQ-1002",
                        Title = "Broken Street Light",
                        Description = "Street light not working on the corner",
                        Priority = RequestPriority.Medium,
                        Status = RequestStatus.Submitted,
                        DateSubmitted = DateTime.Now.AddHours(-5),
                        Location = "Elm Street, Suburb"
                    },
                    new ServiceRequest
                    {
                        RequestId = "REQ-1003",
                        Title = "Garbage Collection Missed",
                        Description = "Garbage not collected on scheduled day",
                        Priority = RequestPriority.Low,
                        Status = RequestStatus.Submitted,
                        DateSubmitted = DateTime.Now.AddHours(-1),
                        Location = "Oak Avenue, North District"
                    }
                };
                
                // Add all sample requests to the queue
                foreach (var request in sampleRequests)
                {
                    _priorityQueue.Enqueue(request);
                }
            }
        }

        /// <summary>
        /// Adds a new request to the priority queue
        /// </summary>
        public void AddRequest(ServiceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            lock (_lock)
            {
                if (!_priorityQueue.Contains(request.RequestId))
                {
                    _priorityQueue.Enqueue(request);
                }
            }
        }

        /// <summary>
        /// Gets the next request to process based on priority
        /// </summary>
        public ServiceRequest GetNextRequest()
        {
            lock (_lock)
            {
                return !_priorityQueue.IsEmpty ? _priorityQueue.Peek() : null;
            }
        }

        /// <summary>
        /// Removes and returns the next request to process
        /// </summary>
        public ServiceRequest ProcessNextRequest()
        {
            lock (_lock)
            {
                return !_priorityQueue.IsEmpty ? _priorityQueue.Dequeue() : null;
            }
        }

        /// <summary>
        /// Updates the priority of an existing request
        /// </summary>
        public bool UpdateRequestPriority(string requestId, RequestPriority newPriority)
        {
            if (string.IsNullOrEmpty(requestId))
                return false;

            lock (_lock)
            {
                if (_priorityQueue.Contains(requestId))
                {
                    _priorityQueue.UpdatePriority(requestId, newPriority);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets all requests in the priority queue (for display purposes)
        /// </summary>
        public List<ServiceRequest> GetAllRequests()
        {
            lock (_lock)
            {
                return _priorityQueue.GetAllRequests().ToList();
            }
        }

        /// <summary>
        /// Removes a request from the priority queue
        /// </summary>
        public bool RemoveRequest(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                return false;

            lock (_lock)
            {
                return _priorityQueue.Remove(requestId);
            }
        }

        /// <summary>
        /// Checks if a request exists in the priority queue
        /// </summary>
        public bool ContainsRequest(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                return false;

            lock (_lock)
            {
                return _priorityQueue.Contains(requestId);
            }
        }

        /// <summary>
        /// Gets the number of requests in the priority queue
        /// </summary>
        public int GetQueueCount()
        {
            lock (_lock)
            {
                return _priorityQueue.Count;
            }
        }

        #region IDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _priorityQueue?.Clear();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
