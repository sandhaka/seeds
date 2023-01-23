namespace Ddd.Domain;

public class Entity<TId> : Aggregate where TId : struct
{
    private int? _requestedHashCode;
    private TId _id;

    public virtual TId Id
    {
        get { return _id; }
        protected set { _id = value; }
    }

    public override bool IsTransient()
    {
        return this.Id.Equals(default(TId));
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Entity<TId>))
            return false;

        if (Object.ReferenceEquals(this, obj))
            return true;

        if (this.GetType() != obj.GetType())
            return false;

        Entity<TId> item = (Entity<TId>)obj;

        return item.Id.Equals(this.Id);                
    }

    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        if (Object.Equals(left, null))
            return (Object.Equals(right, null)) ? true : false;
        else
            return left.Equals(right);
    }

    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            // XOR for random distribution
            // (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)
            if (!_requestedHashCode.HasValue)
                _requestedHashCode = this.Id.GetHashCode() ^ 31;

            return _requestedHashCode.Value;
        }

        return base.GetHashCode();
    }
}