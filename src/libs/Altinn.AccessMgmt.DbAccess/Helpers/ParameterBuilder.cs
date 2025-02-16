using System.Reflection;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Helpers
{
    /// <summary>
    /// Responsible for creating NpgsqlParameters from objects or filters.
    /// </summary>
    public class ParameterBuilder
    {
        /// <summary>
        /// Builds a list of NpgsqlParameters based on the properties of the given object.
        /// </summary>
        public List<NpgsqlParameter> BuildParameters(object obj)
        {
            var parameters = new List<NpgsqlParameter>();
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                parameters.Add(new NpgsqlParameter(property.Name, property.GetValue(obj) ?? DBNull.Value));
            }
            return parameters;
        }

        /// <summary>
        /// Builds translation parameters, including only string properties and the Id.
        /// </summary>
        public List<NpgsqlParameter> BuildTranslationParameters(object obj)
        {
            var parameters = new List<NpgsqlParameter>();
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string) || property.Name == "Id")
                {
                    parameters.Add(new NpgsqlParameter(property.Name, property.GetValue(obj) ?? DBNull.Value));
                }
            }
            return parameters;
        }

        /// <summary>
        /// Builds parameters from a list of filters and RequestOptions.
        /// </summary>
        public List<NpgsqlParameter> BuildFilterParameters(IEnumerable<GenericFilter> filters, RequestOptions options)
        {
            var parameters = new List<NpgsqlParameter>();

            foreach (var filter in filters)
            {
                object value = filter.Comparer switch
                {
                    FilterComparer.StartsWith => $"{filter.Value}%",
                    FilterComparer.EndsWith => $"%{filter.Value}",
                    FilterComparer.Contains => $"%{filter.Value}%",
                    _ => filter.Value
                };

                parameters.Add(new NpgsqlParameter(filter.PropertyName, value));
            }

            if (options.Language != null)
            {
                parameters.Add(new NpgsqlParameter("Language", options.Language));
            }

            if (options.AsOf.HasValue)
            {
                parameters.Add(new NpgsqlParameter("_AsOf", options.AsOf.Value));
            }

            return parameters;
        }
    }
}
