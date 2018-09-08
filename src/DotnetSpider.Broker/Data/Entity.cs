using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Data
{
	public interface IEntity<TPrimary>
	{
		TPrimary Id { get; set; }
	}

	public interface IEntity : IEntity<int>
	{
	}

	/// <summary>
	/// Basic implementation of IEntity interface.
	/// An entity can inherit this class of directly implement to IEntity interface.
	/// </summary>
	/// <typeparam name="TPrimaryKey">Type of the primary key of the entity</typeparam>
	[Serializable]
	public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
	{
		/// <summary>
		/// Unique identifier for this entity.
		/// </summary>
		public virtual TPrimaryKey Id { get; set; }

		/// <summary>
		/// Checks if this entity is transient (it has not an Id).
		/// </summary>
		/// <returns>True, if this entity is transient</returns>
		public virtual bool IsTransient()
		{
			if (EqualityComparer<TPrimaryKey>.Default.Equals(Id, default(TPrimaryKey)))
			{
				return true;
			}

			//Workaround for EF Core since it sets int/long to min value when attaching to dbcontext
			if (typeof(TPrimaryKey) == typeof(int))
			{
				return Convert.ToInt32(Id) <= 0;
			}

			if (typeof(TPrimaryKey) == typeof(long))
			{
				return Convert.ToInt64(Id) <= 0;
			}

			return false;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Entity<TPrimaryKey>))
			{
				return false;
			}

			//Same instances must be considered as equal
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			//Transient objects are not considered as equal
			var other = (Entity<TPrimaryKey>)obj;
			if (IsTransient() && other.IsTransient())
			{
				return false;
			}

			//Must have a IS-A relation of types or must be same type
			var typeOfThis = GetType();
			var typeOfOther = other.GetType();
			if (!typeOfThis.GetTypeInfo().IsAssignableFrom(typeOfOther) && !typeOfOther.GetTypeInfo().IsAssignableFrom(typeOfThis))
			{
				return false;
			}

			return Id.Equals(other.Id);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			if (Id == null)
			{
				return 0;
			}

			return Id.GetHashCode();
		}

		/// <inheritdoc/>
		public static bool operator ==(Entity<TPrimaryKey> left, Entity<TPrimaryKey> right)
		{
			if (Equals(left, null))
			{
				return Equals(right, null);
			}

			return left.Equals(right);
		}

		/// <inheritdoc/>
		public static bool operator !=(Entity<TPrimaryKey> left, Entity<TPrimaryKey> right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[{GetType().Name} {Id}]";
		}
	}

	/// <summary>
	/// A shortcut of <see cref="Entity{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
	/// </summary>
	[Serializable]
	public abstract class Entity : Entity<int>, IEntity
	{
	}

	/// <summary>
	/// An entity can implement this interface if <see cref="CreationTime"/> of this entity must be stored.
	/// <see cref="CreationTime"/> is automatically set when saving <see cref="Entity"/> to database.
	/// </summary>
	public interface IHasCreationTime
	{
		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		DateTime CreationTime { get; set; }
	}

	/// <summary>
	/// This interface is implemented by entities that is wanted to store creation information (who and when created).
	/// Creation time and creator user are automatically set when saving <see cref="Entity"/> to database.
	/// </summary>
	public interface ICreationAudited : IHasCreationTime
	{
		/// <summary>
		/// Id of the creator user of this entity.
		/// </summary>
		long? CreatorUserId { get; set; }
	}

	/// <summary>
	/// An entity can implement this interface if <see cref="LastModificationTime"/> of this entity must be stored.
	/// <see cref="LastModificationTime"/> is automatically set when updating <see cref="Entity"/>.
	/// </summary>
	public interface IHasModificationTime
	{
		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		DateTime? LastModificationTime { get; set; }
	}

	/// <summary>
	/// This interface is implemented by entities that is wanted to store modification information (who and when modified lastly).
	/// Properties are automatically set when updating the <see cref="IEntity"/>.
	/// </summary>
	public interface IModificationAudited : IHasModificationTime
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		long? LastModifierUserId { get; set; }
	}

	/// <summary>
	/// Used to standardize soft deleting entities.
	/// Soft-delete entities are not actually deleted,
	/// marked as IsDeleted = true in the database,
	/// but can not be retrieved to the application.
	/// </summary>
	public interface ISoftDelete
	{
		/// <summary>
		/// Used to mark an Entity as 'Deleted'. 
		/// </summary>
		bool IsDeleted { get; set; }
	}

	/// <summary>
	/// A shortcut of <see cref="CreationAuditedEntity{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
	/// </summary>
	[Serializable]
	public abstract class HasCreationTimeEntity : Entity, IHasCreationTime
	{
		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		public virtual DateTime CreationTime { get; set; }
	}

	/// <summary>
	/// This class can be used to simplify implementing <see cref="ICreationAudited"/>.
	/// </summary>
	/// <typeparam name="TPrimaryKey">Type of the primary key of the entity</typeparam>
	[Serializable]
	public abstract class CreationAuditedEntity<TPrimaryKey> : Entity<TPrimaryKey>, ICreationAudited
	{
		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		public virtual DateTime CreationTime { get; set; }

		/// <summary>
		/// Creator of this entity.
		/// </summary>
		public virtual long? CreatorUserId { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected CreationAuditedEntity()
		{
			CreationTime = DateTime.Now;
		}
	}

	/// <summary>
	/// A shortcut of <see cref="CreationAuditedEntity{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
	/// </summary>
	[Serializable]
	public abstract class CreationAuditedEntity : CreationAuditedEntity<int>, IEntity
	{
	}

	/// <summary>
	/// This interface is implemented by entities which must be audited.
	/// Related properties automatically set when saving/updating <see cref="Entity"/> objects.
	/// </summary>
	public interface IAudited : ICreationAudited, IModificationAudited
	{
	}

	/// <summary>
	/// An entity can implement this interface if <see cref="DeletionTime"/> of this entity must be stored.
	/// <see cref="DeletionTime"/> is automatically set when deleting <see cref="Entity"/>.
	/// </summary>
	public interface IHasDeletionTime : ISoftDelete
	{
		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		DateTime? DeletionTime { get; set; }
	}

	/// <summary>
	/// This interface is implemented by entities which wanted to store deletion information (who and when deleted).
	/// </summary>
	public interface IDeletionAudited : IHasDeletionTime
	{
		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		long? DeleterUserId { get; set; }
	}

	/// <summary>
	/// This class can be used to simplify implementing <see cref="IAudited"/>.
	/// </summary>
	/// <typeparam name="TPrimaryKey">Type of the primary key of the entity</typeparam>
	[Serializable]
	public abstract class AuditedEntity<TPrimaryKey> : CreationAuditedEntity<TPrimaryKey>, IAudited
	{
		/// <summary>
		/// Last modification date of this entity.
		/// </summary>
		public virtual DateTime? LastModificationTime { get; set; }

		/// <summary>
		/// Last modifier user of this entity.
		/// </summary>
		public virtual long? LastModifierUserId { get; set; }
	}

	/// <summary>
	/// This interface ads <see cref="IDeletionAudited"/> to <see cref="IAudited"/> for a fully audited entity.
	/// </summary>
	public interface IFullAudited : IAudited, IDeletionAudited
	{
	}

	/// <summary>
	/// A shortcut of <see cref="FullAuditedEntity{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
	/// </summary>
	[Serializable]
	public abstract class FullAuditedEntity : FullAuditedEntity<int>, IEntity
	{
	}

	/// <summary>
	/// Implements <see cref="IFullAudited"/> to be a base class for full-audited entities.
	/// </summary>
	/// <typeparam name="TPrimaryKey">Type of the primary key of the entity</typeparam>
	[Serializable]
	public abstract class FullAuditedEntity<TPrimaryKey> : AuditedEntity<TPrimaryKey>, IFullAudited
	{
		/// <summary>
		/// Is this entity Deleted?
		/// </summary>
		public virtual bool IsDeleted { get; set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		public virtual long? DeleterUserId { get; set; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		public virtual DateTime? DeletionTime { get; set; }
	}
}
