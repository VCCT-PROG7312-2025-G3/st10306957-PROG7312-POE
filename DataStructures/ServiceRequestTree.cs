using System;
using System.Collections.Generic;
using PROG7312_POE.Models;

namespace PROG7312_POE.DataStructures
{
    // Binary Search Tree for efficient searching of service requests by ID
    public class ServiceRequestTree
    {
        private class TreeNode
        {
            public ServiceRequest Request { get; set; }
            public TreeNode Left { get; set; }
            public TreeNode Right { get; set; }
            public int Height { get; set; } = 1;
        }

        private TreeNode _root;
        private int _count = 0;

        public int Count => _count;

        public void Insert(ServiceRequest request)
        {
            _root = Insert(_root, request);
            _count++;
        }

        private TreeNode Insert(TreeNode node, ServiceRequest request)
        {
            if (node == null)
                return new TreeNode { Request = request };

            int compareResult = string.Compare(request.RequestId, node.Request.RequestId, StringComparison.Ordinal);

            if (compareResult < 0)
                node.Left = Insert(node.Left, request);
            else if (compareResult > 0)
                node.Right = Insert(node.Right, request);
            else
                return node; 

            // Update height
            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

            // Balance the tree
            return Balance(node);
        }

        public ServiceRequest Find(string requestId)
        {
            var node = Find(_root, requestId);
            return node?.Request;
        }

        private TreeNode Find(TreeNode node, string requestId)
        {
            if (node == null)
                return null;

            int compareResult = string.Compare(requestId, node.Request.RequestId, StringComparison.Ordinal);

            if (compareResult < 0)
                return Find(node.Left, requestId);
            if (compareResult > 0)
                return Find(node.Right, requestId);
            
            return node;
        }

        public List<ServiceRequest> GetAll()
        {
            var result = new List<ServiceRequest>();
            InOrderTraversal(_root, result);
            return result;
        }

        private void InOrderTraversal(TreeNode node, List<ServiceRequest> result)
        {
            if (node == null) return;
            
            InOrderTraversal(node.Left, result);
            result.Add(node.Request);
            InOrderTraversal(node.Right, result);
        }

        // AVL Tree balancing methods
        private int GetHeight(TreeNode node) => node?.Height ?? 0;

        private int GetBalanceFactor(TreeNode node)
        {
            if (node == null) return 0;
            return GetHeight(node.Left) - GetHeight(node.Right);
        }

        private TreeNode Balance(TreeNode node)
        {
            int balanceFactor = GetBalanceFactor(node);

            // Left Left Case
            if (balanceFactor > 1 && GetBalanceFactor(node.Left) >= 0)
                return RotateRight(node);

            // Right Right Case
            if (balanceFactor < -1 && GetBalanceFactor(node.Right) <= 0)
                return RotateLeft(node);

            // Left Right Case
            if (balanceFactor > 1 && GetBalanceFactor(node.Left) < 0)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Right Left Case
            if (balanceFactor < -1 && GetBalanceFactor(node.Right) > 0)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        private TreeNode RotateRight(TreeNode y)
        {
            var x = y.Left;
            var T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));
            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));

            return x;
        }

        private TreeNode RotateLeft(TreeNode x)
        {
            var y = x.Right;
            var T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));
            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));

            return y;
        }
    }
}
