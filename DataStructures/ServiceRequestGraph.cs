using System;
using System.Collections.Generic;
using System.Linq;
using PROG7312_POE.Models;

namespace PROG7312_POE.DataStructures
{
    public class ServiceRequestGraph
    {
        private readonly Dictionary<string, List<string>> _adjacencyList;
        private readonly Dictionary<string, ServiceRequest> _requests;

        public ServiceRequestGraph()
        {
            _adjacencyList = new Dictionary<string, List<string>>();
            _requests = new Dictionary<string, ServiceRequest>();
        }

        public void AddRequest(ServiceRequest request)
        {
            if (!_adjacencyList.ContainsKey(request.RequestId))
            {
                _adjacencyList[request.RequestId] = new List<string>();
                _requests[request.RequestId] = request;
            }
        }

        public void AddDependency(string fromRequestId, string toRequestId)
        {
            if (!_adjacencyList.ContainsKey(fromRequestId) || !_adjacencyList.ContainsKey(toRequestId))
                throw new ArgumentException("One or both request IDs not found in the graph");

            if (!_adjacencyList[fromRequestId].Contains(toRequestId))
            {
                _adjacencyList[fromRequestId].Add(toRequestId);
                
                // Add the reverse dependency if it's a bidirectional relationship
                if (!_adjacencyList[toRequestId].Contains(fromRequestId))
                {
                    _adjacencyList[toRequestId].Add(fromRequestId);
                }
            }
        }

        public List<ServiceRequest> GetDependencies(string requestId)
        {
            if (!_adjacencyList.ContainsKey(requestId))
                return new List<ServiceRequest>();

            return _adjacencyList[requestId]
                .Select(id => _requests[id])
                .ToList();
        }

        public List<ServiceRequest> GetDependents(string requestId)
        {
            return _adjacencyList
                .Where(kvp => kvp.Value.Contains(requestId))
                .Select(kvp => _requests[kvp.Key])
                .ToList();
        }

        public List<ServiceRequest> GetAllRelatedRequests(string requestId)
        {
            var visited = new HashSet<string>();
            var result = new List<ServiceRequest>();
            
            if (!_adjacencyList.ContainsKey(requestId))
                return result;

            var queue = new Queue<string>();
            queue.Enqueue(requestId);
            visited.Add(requestId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var currentRequest = _requests[currentId];
                
                if (currentId != requestId) // Don't include the original request
                    result.Add(currentRequest);

                foreach (var neighborId in _adjacencyList[currentId])
                {
                    if (!visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }

            return result;
        }

        public bool HasCycle()
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            foreach (var requestId in _adjacencyList.Keys)
            {
                if (!visited.Contains(requestId) && HasCycleDFS(requestId, visited, recursionStack, null))
                    return true;
            }

            return false;
        }

        private bool HasCycleDFS(string currentId, ISet<string> visited, ISet<string> recursionStack, string parentId)
        {
            visited.Add(currentId);
            recursionStack.Add(currentId);

            foreach (var neighborId in _adjacencyList[currentId])
            {
                if (!visited.Contains(neighborId))
                {
                    if (HasCycleDFS(neighborId, visited, recursionStack, currentId))
                        return true;
                }
                // If the neighbor is in recursion stack and not the parent of current node
                else if (recursionStack.Contains(neighborId) && neighborId != parentId)
                {
                    return true;
                }
            }

            recursionStack.Remove(currentId);
            return false;
        }

        public List<ServiceRequest> TopologicalSort()
        {
            var visited = new HashSet<string>();
            var stack = new Stack<ServiceRequest>();

            foreach (var requestId in _adjacencyList.Keys)
            {
                if (!visited.Contains(requestId))
                {
                    TopologicalSortDFS(requestId, visited, stack);
                }
            }

            return stack.ToList();
        }

        private void TopologicalSortDFS(string requestId, ISet<string> visited, Stack<ServiceRequest> stack)
        {
            visited.Add(requestId);

            foreach (var neighborId in _adjacencyList[requestId])
            {
                if (!visited.Contains(neighborId))
                {
                    TopologicalSortDFS(neighborId, visited, stack);
                }
            }

            stack.Push(_requests[requestId]);
        }
    }
}
