namespace Sodium
{
    using System;

    public sealed class Maybe<T>
    {
        private readonly T value;
        private readonly bool hasValue;

        public Maybe()
        {
            hasValue = false;
        }

        public Maybe(T value)
        {
            this.value = value;
            this.hasValue = true;
        }

        public Maybe(T value, bool hasValue)
        {
            this.value = value;
            this.hasValue = hasValue;
        }

        public static Maybe<T> Null
        {
            get
            {
                return new Maybe<T>(default(T), false);
            }
        }

        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        public static implicit operator T(Maybe<T> m)
        {
            return m.Value();
        }

        public static implicit operator Maybe<T>(T m)
        {
            return new Maybe<T>(m);
        }

        public T Value()
        {
            if (!HasValue)
            {
                throw new ArgumentException("Maybe doesn't contain a value!");
            }

            return value;
        }

        public override string ToString()
        {
            if (HasValue)
            {
                return value.ToString();
            }

            return string.Empty;
        }

        public override bool Equals(object obj)
        {
            var m = obj as Maybe<T>;
            if (m == null)
            {
                return false;
            }

            if (HasValue != m.HasValue)
            { 
                return false;
            }

            return Value().Equals(m.Value());
        }

        public override int GetHashCode()
        {
            return !HasValue ? 0 : Value().GetHashCode();
        }
    }
}
