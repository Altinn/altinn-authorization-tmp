using System.Linq.Expressions;
using System.Reflection;
using Altinn.AccessMgmt.DbAccess.Models;

namespace Altinn.AccessMgmt.DbAccess.Helpers
{
    /// <summary>
    /// Provides a fluent API for building a <see cref="DbDefinition"/> for the entity type <typeparamref name="T"/>.
    /// This builder allows configuration of basic, extended, and cross-reference properties for the database definition.
    /// </summary>
    /// <typeparam name="T">The entity type for which the definition is being built.</typeparam>
    public sealed class DefinitionBuilder<T>
    {
        /// <summary>
        /// The underlying database definition that is being constructed.
        /// </summary>
        private DbDefinition dbDefinition { get; set; } = new(typeof(T));

        /// <summary>
        /// Returns the built <see cref="DbDefinition"/> after configuration.
        /// </summary>
        /// <returns>The configured <see cref="DbDefinition"/>.</returns>
        public DbDefinition Build()
        {
            return dbDefinition;
        }

        #region Basic

        /// <summary>
        /// Sets whether the entity supports translation.
        /// </summary>
        /// <param name="value">If set to <c>true</c>, translations are enabled; otherwise, they are disabled.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> SetTranslation(bool value = true)
        {
            dbDefinition.HasTranslation = value;
            return this;
        }

        /// <summary>
        /// Sets whether the entity supports history tracking.
        /// </summary>
        /// <param name="value">If set to <c>true</c>, history is enabled; otherwise, it is disabled.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> SetHistory(bool value = true)
        {
            dbDefinition.HasHistory = value;
            return this;
        }

        /// <summary>
        /// Registers a property as a column in the database definition.
        /// </summary>
        /// <param name="column">An expression that identifies the property to register.</param>
        /// <param name="nullable">Indicates whether the column can contain null values.</param>
        /// <param name="defaultValue">The default value for the column, if any.</param>
        /// <param name="length">The maximum length of the column (if applicable).</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> RegisterProperty(Expression<Func<T, object>> column, bool nullable = false, string? defaultValue = null, int? length = null)
        {
            var propertyInfo = ExtractPropertyInfo(column);
            var columnDef = new ColumnDefinition()
            {
                Name = propertyInfo.Name,
                Property = propertyInfo,
                DefaultValue = defaultValue,
                IsNullable = nullable,
                Length = length
            };
            dbDefinition.Columns.Add(columnDef);

            return this;
        }

        /// <summary>
        /// Registers one or more properties as the primary key for the entity.
        /// </summary>
        /// <param name="properties">A collection of expressions identifying the properties that form the primary key.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        /// <exception cref="Exception">Thrown if any of the specified properties does not exist on the entity type.</exception>
        public DefinitionBuilder<T> RegisterPrimaryKey(IEnumerable<Expression<Func<T, object?>>> properties)
        {
            var propertyNames = new List<string>();
            var propertyInfos = typeof(T).GetProperties().ToList();
            foreach (var property in properties)
            {
                var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
                propertyNames.Add(propertyName);

                if (!propertyInfos.Exists(t => t.Name == propertyName))
                {
                    throw new Exception($"{typeof(T).Name} does not contain the property '{propertyName}'");
                }
            }

            var name = $"PK_{typeof(T).Name}";

            dbDefinition.UniqueConstraints.Add(new ConstraintDefinition()
            {
                Name = name,
                Type = typeof(T),
                Columns = propertyNames,
                IsUnique = true,
            });

            return this;
        }

        /// <summary>
        /// Registers a unique constraint for the specified properties.
        /// </summary>
        /// <param name="properties">A collection of expressions identifying the properties that should have a unique constraint.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        /// <exception cref="Exception">Thrown if any of the specified properties does not exist on the entity type.</exception>
        public DefinitionBuilder<T> RegisterUniqueConstraint(IEnumerable<Expression<Func<T, object?>>> properties)
        {
            var propertyNames = new List<string>();
            var propertyInfos = typeof(T).GetProperties().ToList();
            foreach (var property in properties)
            {
                var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
                propertyNames.Add(propertyName);

                if (!propertyInfos.Exists(t => t.Name == propertyName))
                {
                    throw new Exception($"{typeof(T).Name} does not contain the property '{propertyName}'");
                }
            }

            var name = $"UC_{typeof(T).Name}_{string.Join("_", propertyNames)}";

            dbDefinition.UniqueConstraints.Add(new ConstraintDefinition()
            {
                Name = name,
                Type = typeof(T),
                Columns = propertyNames,
                IsUnique = true,
            });

            return this;
        }

        #endregion

        #region Extended

        /// <summary>
        /// Registers an extended property that defines a relationship between the primary entity and an extended entity.
        /// </summary>
        /// <typeparam name="TExtended">The type of the extended entity.</typeparam>
        /// <typeparam name="TJoin">The type used for joining the entities.</typeparam>
        /// <param name="TProperty">An expression selecting the property on the primary entity.</param>
        /// <param name="TJoinProperty">An expression selecting the property on the join entity.</param>
        /// <param name="TExtendedProperty">An expression selecting the property on the extended entity.</param>
        /// <param name="optional">Indicates whether the relationship is optional.</param>
        /// <param name="isList">Indicates whether the relationship represents a collection.</param>
        /// <param name="cascadeDelete">Indicates whether cascade delete should be used for the relationship.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> RegisterExtendedProperty<TExtended, TJoin>(
            Expression<Func<T, object>> TProperty,
            Expression<Func<TJoin, object>> TJoinProperty,
            Expression<Func<TExtended, object>> TExtendedProperty,
            bool optional = false,
            bool isList = false,
            bool cascadeDelete = false)
        {
            string baseProperty = ExtractPropertyInfo(TProperty).Name;
            string refProperty = ExtractPropertyInfo(TJoinProperty).Name;
            string extendedProperty = ExtractPropertyInfo(TExtendedProperty).Name;

            var join = new ForeignKeyDefinition()
            {
                Name = $"FK_{typeof(T).Name}_{extendedProperty}_{typeof(TJoin).Name}",
                Base = typeof(T),
                Ref = typeof(TJoin),
                BaseProperty = baseProperty,
                RefProperty = refProperty,
                ExtendedProperty = extendedProperty,
                IsOptional = optional,
                IsList = isList,
                UseCascadeDelete = cascadeDelete
            };
            dbDefinition.ForeignKeys.Add(join);

            return this;
        }

        /// <summary>
        /// Registers a relation between the primary entity and a join entity, defining how they are connected.
        /// </summary>
        /// <typeparam name="TExtended">The type of the extended entity that represents additional data.</typeparam>
        /// <typeparam name="TJoin">The type of the join entity used to establish the relation.</typeparam>
        /// <param name="TProperty">An expression selecting the property on the primary entity.</param>
        /// <param name="TJoinProperty">An expression selecting the property on the join entity.</param>
        /// <param name="TExtendedProperty">An expression selecting the property on the extended entity.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> RegisterRelation<TExtended, TJoin>(
            Expression<Func<T, object>> TProperty,
            Expression<Func<TJoin, object>> TJoinProperty,
            Expression<Func<TExtended, object>> TExtendedProperty)
        {
            string baseProperty = ExtractPropertyInfo(TProperty).Name;
            string refProperty = ExtractPropertyInfo(TJoinProperty).Name;
            string extendedProperty = ExtractPropertyInfo(TExtendedProperty).Name;

            var relation = new RelationDefinition()
            {
                Base = typeof(T),
                Ref = typeof(TJoin),
                BaseProperty = baseProperty,
                RefProperty = refProperty,
                ExtendedProperty = extendedProperty
            };
            dbDefinition.Relations.Add(relation);

            return this;
        }

        #endregion

        #region Cross

        /// <summary>
        /// Registers a cross-reference between two sets of related entities.
        /// </summary>
        /// <typeparam name="TA">The type of the first related entity.</typeparam>
        /// <typeparam name="TB">The type of the second related entity.</typeparam>
        /// <param name="TASourceProperty">An expression selecting the source property for the first related entity.</param>
        /// <param name="TAJoinProperty">An expression selecting the join property on the first related entity.</param>
        /// <param name="TBSourceProperty">An expression selecting the source property for the second related entity.</param>
        /// <param name="TBJoinProperty">An expression selecting the join property on the second related entity.</param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> RegisterAsCrossReference<TA, TB>(
            Expression<Func<T, object>> TASourceProperty,
            Expression<Func<TA, object>> TAJoinProperty,
            Expression<Func<T, object>> TBSourceProperty,
            Expression<Func<TB, object>> TBJoinProperty)
        {
            // TODO: Implement cross-reference registration logic.
            return this;
        }

        /// <summary>
        /// Registers cross-reference relationships with extended properties for both related entity sets.
        /// This method internally registers extended properties for both sides and then defines the cross-reference.
        /// </summary>
        /// <typeparam name="TExtended">The type of the extended entity.</typeparam>
        /// <typeparam name="TA">The type of the first related entity.</typeparam>
        /// <typeparam name="TB">The type of the second related entity.</typeparam>
        /// <param name="defineA">
        /// A tuple containing expressions for the primary source property, join property, and extended property for the first related entity.
        /// </param>
        /// <param name="defineB">
        /// A tuple containing expressions for the primary source property, join property, and extended property for the second related entity.
        /// </param>
        /// <returns>The current <see cref="DefinitionBuilder{T}"/> instance for fluent chaining.</returns>
        public DefinitionBuilder<T> RegisterAsCrossReferenceExtended<TExtended, TA, TB>
        (
            (Expression<Func<T, object>> Source, Expression<Func<TA, object>> Join, Expression<Func<TExtended, object>> Extended) defineA,
            (Expression<Func<T, object>> Source, Expression<Func<TB, object>> Join, Expression<Func<TExtended, object>> Extended) defineB
        )
        {
            RegisterExtendedProperty(defineA.Source, defineA.Join, defineA.Extended);
            RegisterExtendedProperty(defineB.Source, defineB.Join, defineB.Extended);
            RegisterAsCrossReference(defineA.Source, defineA.Join, defineB.Source, defineB.Join);

            return this;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Extracts the <see cref="PropertyInfo"/> from the given property expression.
        /// </summary>
        /// <typeparam name="TLocal">The type of the object containing the property.</typeparam>
        /// <param name="expression">An expression that selects a property of <typeparamref name="TLocal"/>.</param>
        /// <returns>The <see cref="PropertyInfo"/> corresponding to the property in the expression.</returns>
        /// <exception cref="ArgumentException">Thrown if the expression does not refer to a valid property.</exception>
        private PropertyInfo ExtractPropertyInfo<TLocal>(Expression<Func<TLocal, object>> expression)
        {
            MemberExpression memberExpression;

            if (expression.Body is MemberExpression)
            {
                memberExpression = (MemberExpression)expression.Body;
            }
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression)
            {
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                throw new ArgumentException("Expression must refer to a property.");
            }

            return memberExpression.Member as PropertyInfo ?? throw new ArgumentException("Member is not a property.");
        }

        #endregion
    }
}
