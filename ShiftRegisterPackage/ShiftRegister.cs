using System.Collections.Generic;

using Zebra.ADA.Core.Steps;
using Zebra.ADA.Core.Steps.Annotations;
using Zebra.ADA.Core.Steps.Attributes;

namespace ShiftRegisterPackage
{
    /// <summary>
    /// Implements a shift register for numeric values.
    /// </summary>
    /// <remarks>
    /// Behavior per run:
    /// <list type="number">
    /// <item><description>If <see cref="Clear"/> is true, the internal buffer is cleared and no value is inserted.</description></item>
    /// <item><description>Otherwise <see cref="InValue"/> is inserted at the beginning of the buffer (most-recent-first).</description></item>
    /// <item><description>If <see cref="Infinite"/> is false, the buffer is trimmed to <see cref="MaxSize"/>.</description></item>
    /// </list>
    /// </remarks>
    [UIEditor("ShiftRegisterPackage.ShiftRegisterUIEditor")]
    public class ShiftRegister : Step, IInternalCounter
    {
        private readonly ShiftRegisterBuffer _buffer = new();

        #region Inputs

        /// <summary>
        /// Gets the input value inserted into the shift register on each run (unless <see cref="Clear"/> is true).
        /// </summary>
        [Input]
        [Linkable]
        public double InValue => (double)GetInputValue(nameof(InValue));

        /// <summary>
        /// Gets the maximum buffer size when <see cref="Infinite"/> is false.
        /// </summary>
        /// <remarks>
        /// Values smaller than 1 are treated as invalid; the buffer is cleared and the run ends.
        /// </remarks>
        [Input]
        [Linkable]
        public int MaxSize => (int)GetInputValue(nameof(MaxSize));

        /// <summary>
        /// Gets a value indicating whether the buffer should grow without trimming.
        /// </summary>
        [Input]
        [Linkable]
        public bool Infinite => (bool)GetInputValue(nameof(Infinite));

        /// <summary>
        /// Gets a value indicating whether the buffer should be cleared for this run (one-shot).
        /// </summary>
        /// <remarks>
        /// This step cannot modify the input itself. The "auto reset to false" behavior is achieved by
        /// treating <see cref="Clear"/> as a one-shot signal: when true, the buffer is cleared and the run exits.
        /// On the next run, <see cref="Clear"/> is evaluated again from its linked expression/source.
        /// </remarks>
        [Input]
        [Linkable]
        public bool Clear => (bool)GetInputValue(nameof(Clear));

        #endregion

        #region Execution

        /// <inheritdoc />
        protected override void Run()
        {
            // One-shot clear: clear state and exit without inserting the current value.
            if (Clear)
            {
                _buffer.Clear();
                return;
            }

            // If size-limited, validate MaxSize (minimum 1).
            if (!Infinite && MaxSize < 1)
            {
                _buffer.Clear();
                return;
            }

            // Insert newest value at the start (most-recent-first).
            _buffer.InsertAtStart(InValue);

            // Trim if not infinite.
            if (!Infinite)
            {
                _buffer.TrimToMax(MaxSize);
            }
        }

        #endregion

        #region Outputs

        /// <summary>
        /// Gets a snapshot of the current buffer as a numeric array.
        /// </summary>
        /// <remarks>
        /// A <see cref="double"/> array is used so ADA can treat it as a native numeric array for expression functions.
        /// The returned array is a snapshot; modifying it will not affect the internal buffer.
        /// </remarks>
        [Output]
        public double[] Values
        {
            get
            {
                ValidateOutputAvailability(nameof(Values));
                return _buffer.ToArray();
            }
        }

        /// <summary>
        /// Gets the current number of elements in the shift register.
        /// </summary>
        [Output]
        public int Count
        {
            get
            {
                ValidateOutputAvailability(nameof(Count));
                return _buffer.Count;
            }
        }

        #endregion

        #region IInternalCounter

        /// <inheritdoc />
        public void ResetInternalCounter() => _buffer.Clear();

        /// <inheritdoc />
        public void GetInternalCounter(ref Dictionary<string, int> values)
        {
            values ??= new Dictionary<string, int>();
            values[InternalCounterKeys.Count] = _buffer.Count;
        }

        /// <inheritdoc />
        public void SetInternalCounter(Dictionary<string, int> values)
        {
            // Intentionally left blank.
            // If you later decide to restore state from external values, implement it here.
        }

        #endregion
    }
}
