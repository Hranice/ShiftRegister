using System;
using System.Collections.Generic;

namespace ShiftRegisterPackage
{
    /// <summary>
    /// Encapsulates the internal storage and operations of the shift register.
    /// </summary>
    internal sealed class ShiftRegisterBuffer
    {
        private readonly List<double> _data = new();

        /// <summary>
        /// Gets the number of elements currently stored.
        /// </summary>
        public int Count => _data.Count;

        /// <summary>
        /// Clears the buffer.
        /// </summary>
        public void Clear() => _data.Clear();

        /// <summary>
        /// Inserts a value as the newest element (index 0), shifting older values to the right.
        /// </summary>
        /// <param name="value">The value to insert.</param>
        public void InsertAtStart(double value) => _data.Insert(0, value);

        /// <summary>
        /// Trims the buffer to the specified maximum size.
        /// </summary>
        /// <param name="maxSize">Maximum number of elements to keep. Must be non-negative.</param>
        public void TrimToMax(int maxSize)
        {
            if (maxSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "maxSize must be non-negative.");
            }

            if (_data.Count <= maxSize)
            {
                return;
            }

            _data.RemoveRange(maxSize, _data.Count - maxSize);
        }

        /// <summary>
        /// Creates a snapshot array of the current buffer content.
        /// </summary>
        public double[] ToArray() => _data.ToArray();
    }
}
