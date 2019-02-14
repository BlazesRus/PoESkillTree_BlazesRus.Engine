﻿using System;

namespace PoESkillTree.Computation.Core.Events
{
    public static class BufferingEventViewProvider
    {
        /// <summary>
        /// Creates an <see cref="IBufferingEventViewProvider{T}"/> using <paramref name="defaultView"/> as
        /// <see cref="IBufferingEventViewProvider{T}.DefaultView"/>, <paramref name="bufferingView"/> as
        /// <see cref="IBufferingEventViewProvider{T}.BufferingView"/>, and both to calculate
        /// <see cref="ICountsSubsribers.SubscriberCount"/>.
        /// </summary>
        public static IBufferingEventViewProvider<T> Create<T>(T defaultView, T bufferingView)
            where T : ICountsSubsribers
        {
            return new BufferingEventViewProvider<T>(defaultView, bufferingView, 
                () => defaultView.SubscriberCount + bufferingView.SubscriberCount);
        }
    }


    /// <inheritdoc />
    /// <summary>
    /// Trivial implementation of <see cref="IBufferingEventViewProvider{T}" /> that gets all the required parameters
    /// passed to it through the constructor.
    /// </summary>
    public class BufferingEventViewProvider<T> : IBufferingEventViewProvider<T>
    {
        private readonly Func<int> _countSubscribers;

        public BufferingEventViewProvider(T defaultView, T bufferingView, Func<int> countSubscribers)
        {
            _countSubscribers = countSubscribers;
            DefaultView = defaultView;
            BufferingView = bufferingView;
        }

        public T DefaultView { get; }
        public T BufferingView { get; }
        public int SubscriberCount => _countSubscribers();
    }
}