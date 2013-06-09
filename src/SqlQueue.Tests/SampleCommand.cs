namespace SqlQueue.Tests
{
    public class SampleCommand
    {
        public string SomeContents { get; set; }

        protected bool Equals(SampleCommand other)
        {
            return string.Equals(SomeContents, other.SomeContents);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SampleCommand) obj);
        }

        public override int GetHashCode()
        {
            return (SomeContents != null ? SomeContents.GetHashCode() : 0);
        }
    }
}