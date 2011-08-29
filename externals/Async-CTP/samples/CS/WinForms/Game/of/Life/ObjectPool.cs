//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ObjectPool.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GameOfLife
{
    /// <summary>Provides a thread-safe object pool.</summary>
    /// <typeparam name="T">Specifies the type of the elements stored in the pool.</typeparam>
    [DebuggerDisplay("Count={Count}")]
    public sealed class ObjectPool<T>
    {
        private readonly Func<T> m_generator;
        private readonly IProducerConsumerCollection<T> m_objects = new ConcurrentQueue<T>();

        /// <summary>Initializes an instance of the ObjectPool class.</summary>
        /// <param name="generator">The function used to create items when no items exist in the pool.</param>
        public ObjectPool(Func<T> generator)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            m_generator = generator;
        }

        /// <summary>Adds the provided item into the pool.</summary>
        /// <param name="item">The item to be added.</param>
        public void PutObject(T item) { m_objects.TryAdd(item); }

        /// <summary>Gets an item from the pool.</summary>
        /// <returns>The removed or created item.</returns>
        /// <remarks>If the pool is empty, a new item will be created and returned.</remarks>
        public T GetObject()
        {
            T value;
            if (m_objects.TryTake(out value)) return value;
            else return m_generator();
        }
    }
}