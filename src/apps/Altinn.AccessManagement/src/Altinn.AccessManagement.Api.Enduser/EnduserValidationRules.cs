using Altinn.AccessMgmt.Core.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Errors;

/// <summary>
/// A utility class that provides methods for validating data using a series of rules.
/// </summary>
public static class ValidationRules
{
    /// <summary>
    /// A delegate representing a validation rule that accepts a reference to a <see cref="ValidationErrorBuilder"/>
    /// and adds errors if validation fails.
    /// </summary>
    /// <param name="errors">The reference to the <see cref="ValidationErrorBuilder"/> where validation errors are added.</param>
    public delegate void Func(ref ValidationErrorBuilder errors);

    /// <summary>
    /// A delegate that returns a validation rule that accepts a reference to a <see cref="ValidationErrorBuilder"/>
    /// and adds errors if validation fails.
    /// </summary>
    public delegate Func FuncExpression();

    /// <summary>
    /// Validates a series of rules against the provided parameters.
    /// It executes each rule in sequence and returns a <see cref="ValidationProblemInstance"/>
    /// if any validation errors are found, or <c>null</c> if no errors exist.
    /// </summary>
    /// <param name="rules">An array of validation rules to apply.</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> containing validation errors, if any,
    /// or <c>null</c> if the validation passed without any errors.
    /// </returns>
    public static ValidationProblemInstance? Validate(params FuncExpression[] rules)
    {
        var builder = default(ValidationErrorBuilder);
        All(rules)()(ref builder);
        builder.TryBuild(out var result);
        return result;
    }

    /// <summary>
    /// Validates a series of rules against the provided parameters.
    /// It executes each rule in sequence and returns a <see cref="ValidationProblemInstance"/>
    /// if any validation errors are found, or <c>null</c> if no errors exist.
    /// </summary>
    /// <param name="rules">An array of validation rules to apply.</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> containing validation errors, if any,
    /// or <c>null</c> if the validation passed without any errors.
    /// </returns>
    public static ValidationProblemInstance? Validate(params Func[] rules)
    {
        var builder = default(ValidationErrorBuilder);
        foreach (var rule in rules)
        {
            rule(ref builder);
        }

        builder.TryBuild(out var result);

        return result;
    }

    /// <summary>
    /// Combines multiple validation rules that must all pass.
    /// </summary>
    /// <param name="funcs">The validation functions to combine.</param>
    /// <returns>A combined validation rule that applies all the specified rules.</returns>
    public static FuncExpression All(params FuncExpression[] funcs) => () =>
    {
        var results = new List<Func>();
        foreach (var func in funcs)
        {
            if (func() is var fn && fn is { })
            {
                results.Add(fn);
            }
        }

        return (ref ValidationErrorBuilder errors) =>
        {
            foreach (var result in results)
            {
                result(ref errors);
            }
        };
    };

    /// <summary>
    /// Combines multiple validation rules where at least one must pass.
    /// </summary>
    /// <param name="funcs">The validation functions to combine.</param>
    /// <returns>A combined validation rule that applies any of the specified rules.</returns>
    public static FuncExpression Any(params FuncExpression[] funcs) => () =>
    {
        var results = new List<Func>();
        foreach (var func in funcs)
        {
            if (func() is var fn && fn is { })
            {
                results.Add(fn);
            }
        }

        if (results.Count == funcs.Length)
        {
            return (ref ValidationErrorBuilder errors) =>
            {
                foreach (var result in results)
                {
                    result(ref errors);
                }
            };
        }

        return null;
    };

    /// <summary>
    /// Validates the default query parameters for an end user.
    /// </summary>
    /// <param name="userUuid">The UUID of the user performing the action (current user).</param>
    /// <param name="party">The UUID of the acting party (the user acting on behalf). Can also be "me".</param>
    /// <param name="from">The UUID of the 'from' party (could be a person or an organization).</param>
    /// <param name="to">The UUID of the 'to' party (could be a person or an organization).</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> with validation errors if any.
    /// </returns>
    public static ValidationProblemInstance EnduserAddConnection(Guid userUuid, string party, string from, string to) => Validate(
        QueryParameters.Party(party),
        Any(
            QueryParameters.PartyFrom(from),
            QueryParameters.PartyTo(to)
        ),
        QueryParameters.EnduserCreateCombination(userUuid, party, from, to)
    );

    /// <summary>
    /// Validates the default query parameters for an end user.
    /// </summary>
    /// <param name="userUuid">The UUID of the user performing the action (current user).</param>
    /// <param name="party">The UUID of the acting party (the user acting on behalf). Can also be "me".</param>
    /// <param name="from">The UUID of the 'from' party (could be a person or an organization).</param>
    /// <param name="to">The UUID of the 'to' party (could be a person or an organization).</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> with validation errors if any.
    /// </returns>
    public static ValidationProblemInstance EnduserGetConnection(Guid userUuid, string party, string from, string to) => Validate(
        QueryParameters.Party(party),
        QueryParameters.EnduserReadInputCombination(userUuid, party, from, to),
        Any(
            QueryParameters.PartyFrom(from),
            QueryParameters.PartyTo(to)
        )
    );

    /// <summary>
    /// Validates the default query parameters for an end user.
    /// </summary>
    /// <param name="userUuid">The UUID of the user performing the action (current user).</param>
    /// <param name="party">The UUID of the acting party (the user acting on behalf). Can also be "me".</param>
    /// <param name="from">The UUID of the 'from' party (could be a person or an organization).</param>
    /// <param name="to">The UUID of the 'to' party (could be a person or an organization).</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> with validation errors if any.
    /// </returns>
    public static ValidationProblemInstance EnduserRemoveConnection(Guid userUuid, string party, string from, string to) => Validate(
        QueryParameters.Party(party),
        Any(
            QueryParameters.PartyFrom(from),
            QueryParameters.PartyTo(to)
        ),
        QueryParameters.EnduserDeleteCombination(userUuid, party, from, to)
    );

    /// <summary>
    /// Provides validation rules for query parameters.
    /// </summary>
    public static class QueryParameters
    {
        private static IEnumerable<string> ParamKeywords { get; } = ["me", "all"];

        private static string ParamKeywordsToString { get; } = "<me, all | blank, uuid>";

        /// <summary>
        /// Checks if any packages exist with given ID.
        /// </summary>
        /// <param name="packages">Lists of packages</param>
        /// <param name="paramName">name of query parameter</param>
        /// <returns></returns>
        public static FuncExpression AnyPackages(IEnumerable<ConnectionPackage> packages, string paramName = "packageId") => () =>
        {
            ArgumentNullException.ThrowIfNull(packages);
            ArgumentException.ThrowIfNullOrEmpty(paramName);

            if (packages.Any())
            {
                return null;
            }

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidResource, $"QUERY/{paramName}", [new("Packages", $"No packages found.")]);
        };

        /// <summary>
        /// Checks the list of packages all <see cref="Package.IsAssignable"/> is set to true.
        /// </summary>
        /// <param name="packages">list of packages</param>
        /// <param name="paramName">name of the query parameter</param>
        /// <returns></returns>
        public static FuncExpression PackageIsAssignableByDefinition(IEnumerable<ExtConnectionPackage> packages, string paramName = "packageId") => () =>
        {
            ArgumentNullException.ThrowIfNull(packages);
            ArgumentException.ThrowIfNullOrEmpty(paramName);

            if (packages.All(t => t.Package.IsAssignable))
            {
                return null;
            }

            var packagesNotAssignable = packages
                .Where(p => p.CanAssign)
                .Select(p => p.Id);

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new("Packages", $"{string.Join(",", packagesNotAssignable)} are not assignable.")]);
        };

        /// <summary>
        /// Checks is packages can be asigned by checking <see cref="ConnectionPackage.CanAssign"/> is set to to true.
        /// </summary>
        /// <param name="packages">List of packages.</param>
        /// <param name="paramName">name of the query parameter.</param>
        /// <returns></returns>
        public static FuncExpression PackageIsAssignableByUser(IEnumerable<ConnectionPackage> packages, string paramName = "packageId") => () =>
        {
            ArgumentNullException.ThrowIfNull(packages);
            ArgumentException.ThrowIfNullOrEmpty(paramName);

            if (packages.All(t => t.CanAssign))
            {
                return null;
            }

            var packagesNotAssignableByUser = packages
                .Where(p => p.CanAssign)
                .Select(p => p.Id);

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new("Packages", $"Packages with IDs {string.Join(",", packagesNotAssignableByUser)} can't be assigned by user.")]);
        };

        /// <summary>
        /// Used to check if package exists by check URN and resulkt of the DB lookup.
        /// </summary>
        /// <param name="packages">List of packags</param>
        /// <param name="paramName">name of the query URN parameter.</param>
        /// <returns></returns>
        public static FuncExpression PackageUrnLookup(IEnumerable<Package> packages, string paramName = "packageUrn") => () =>
        {
            ArgumentNullException.ThrowIfNull(packages);
            ArgumentException.ThrowIfNullOrEmpty(paramName);

            if (packages?.Count() == 1)
            {
                return null;
            }

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, $"QUERY/{paramName}", [new("packages", string.Join(",", packages.Select(p => p.Id.ToString())))]);
        };

        /// <summary>
        /// Checks if any packages are assigned. Used along with cascade delete
        /// </summary>
        /// <param name="packages">List of packages.</param>
        /// <param name="paramName">name of the query parameter.</param>
        public static FuncExpression HasPackagesAssigned(IEnumerable<AssignmentPackage> packages, string paramName = "cascade") => () =>
        {
            ArgumentNullException.ThrowIfNull(packages);
            ArgumentException.ThrowIfNullOrEmpty(paramName);

            if (packages.Any())
            {
                return (ref ValidationErrorBuilder errors) =>
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, $"QUERY/{paramName}", [new("packages", $"following packages has active assignments [{string.Join(",", packages.Select(p => p.Id.ToString()))}].")]);
            }

            return null;
        };

        /// <summary>
        /// Checks if any delegations are given assigned. Used along with cascade delete
        /// </summary>
        /// <param name="delegations">List of delegations.</param>
        /// <param name="paramName">name of the query parameter.</param>
        public static FuncExpression HasDelegationsAssigned(IEnumerable<Delegation> delegations, string paramName = "cascade") => () =>
        {
            ArgumentNullException.ThrowIfNull(delegations);
            if (delegations.Any())
            {
                return (ref ValidationErrorBuilder errors) =>
                    errors.Add(ValidationErrors.EntityNotExists, $"QUERY/{paramName}", [new("packages", string.Join(",", delegations.Select(p => p.Id.ToString())))]);
            }

            return null;
        };

        /// <summary>
        /// Checks if party exists
        /// </summary>
        /// <param name="party"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static FuncExpression PartyExists(Entity party, string paramName = "party") => () =>
        {
            if (party is { })
            {
                return null;
            }

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.EntityNotExists, $"QUERY/{paramName}", [new("party", $"Entity do not exists.")]);
        };

        public static FuncExpression PartyIsEntityType(ExtEntity party, string entityType, string paramName = "party") => () =>
        {
            if (party is { })
            {
                if (party.Type.Name.Equals(entityType, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
            }

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.EntityNotExists, $"QUERY/{paramName}", [new("party", $"Entity do not exists or entity is not of type '{entityType}'.")]);
        };

        /// <summary>
        /// Validates combination of input parameters
        /// </summary>
        public static FuncExpression EnduserCreateCombination(Guid useruuid, string party, string from, string to) => () =>
        {
            if (!string.IsNullOrEmpty(party) && party.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserDeleteCombination(useruuid, useruuid.ToString(), from, to)();
            }

            if (!string.IsNullOrEmpty(from) && from.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserDeleteCombination(useruuid, party, useruuid.ToString(), to)();
            }

            if (!Guid.TryParse(party, out var partyUuid))
            {
                return (ref ValidationErrorBuilder errors) =>
                    errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/party", [new("party", $"The 'party' parameter is not valid.")]);
            }

            if (!string.IsNullOrEmpty(from) && Guid.TryParse(from, out var fromUuid) && fromUuid == partyUuid)
            {
                return null;
            }

            return (ref ValidationErrorBuilder errors) =>
            {
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/from", [new("from", "must match the 'party' UUID.")]);
            };
        };

        /// <summary>
        /// Validates that the acting party matches either the 'from' or 'to' party,
        /// and ensures that none of the provided UUIDs are invalid or empty. If any UUID is 
        /// set to "me", it will be replaced by the user's UUID.
        /// </summary>
        /// <param name="useruuid">The UUID of the user performing the action (current user).</param>
        /// <param name="party">The UUID of the acting party (the user acting on behalf). Can also be "me".</param>
        /// <param name="from">The UUID of the 'from' party (could be a person or an organization).</param>
        /// <param name="to">The UUID of the 'to' party (could be a person or an organization).</param>
        /// <returns>
        /// A validation rule that adds an error if any UUID is invalid or empty, 
        /// or if the party does not match either the 'from' or 'to' UUID.
        /// </returns>
        public static FuncExpression EnduserReadInputCombination(Guid useruuid, string party, string from, string to) => () =>
        {
            if (!string.IsNullOrEmpty(party) && party.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserReadInputCombination(useruuid, useruuid.ToString(), from, to)();
            }

            if (!string.IsNullOrEmpty(from) && from.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserReadInputCombination(useruuid, party, useruuid.ToString(), to)();
            }

            if (!string.IsNullOrEmpty(to) && to.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserReadInputCombination(useruuid, party, from, useruuid.ToString())();
            }

            if (!Guid.TryParse(party, out var partyUuid))
            {
                return (ref ValidationErrorBuilder errors) =>
                    errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/party", [new("party", $"The 'party' parameter is not valid.")]);
            }

            if (!string.IsNullOrEmpty(from) && Guid.TryParse(from, out var fromUuid) && fromUuid == partyUuid)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(to) && Guid.TryParse(to, out var toUuid) && toUuid == partyUuid)
            {
                return null;
            }

            return (ref ValidationErrorBuilder errors) =>
            {
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/from", [new("from", "Either the 'from' UUID or the 'to' UUID must match the 'party' UUID. Neither matches the 'party' UUID.")]);
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/to", [new("to", "Either the 'to' UUID or the 'from' UUID must match the 'party' UUID. Neither matches the 'party' UUID.")]);
            };
        };

        /// <summary>
        /// Delete combination
        /// </summary>
        /// <param name="useruuid"></param>
        /// <param name="party"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static FuncExpression EnduserDeleteCombination(Guid useruuid, string party, string from, string to) => () =>
        {
            if (!string.IsNullOrEmpty(party) && party.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserDeleteCombination(useruuid, useruuid.ToString(), from, to)();
            }

            if (!string.IsNullOrEmpty(from) && from.Equals("me", StringComparison.InvariantCultureIgnoreCase))
            {
                return EnduserDeleteCombination(useruuid, party, useruuid.ToString(), to)();
            }

            if (!Guid.TryParse(party, out var partyUuid))
            {
                return (ref ValidationErrorBuilder errors) =>
                    errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/party", [new("party", $"The 'party' parameter is not valid.")]);
            }

            if (!string.IsNullOrEmpty(from) && Guid.TryParse(from, out var fromUuid) && fromUuid == partyUuid)
            {
                return null;
            }

            return (ref ValidationErrorBuilder errors) =>
            {
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/from", [new("from", "must match the 'party' UUID.")]);
            };
        };

        /// <summary>
        /// Validates that a party query parameter is either a valid UUID
        /// or matches one of the predefined keywords (e.g., "me", "all") <see cref="ParamKeywords"/>.
        /// </summary>
        /// <param name="party">The value of the party query parameter to validate.</param>
        /// <param name="paramName">The name of the query parameter (used for error reporting).</param>
        /// <param name="values"></param>
        /// <returns>
        /// A validation rule that adds an error if the party value is not a valid UUID
        /// and does not match any of the predefined keywords <see cref="ParamKeywords"/>.
        /// </returns>
        public static FuncExpression PartyIs(string party, string paramName = "party", params string[] values) => () =>
        {
            if (Guid.TryParse(party, out var _))
            {
                return null;
            }

            foreach (var value in values)
            {
                if (string.Equals(party, value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
            }

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new("party", $"Party '{party}' must be a valid UUID or one of <{ParamKeywordsToString}>.")]);
        };

        /// <summary>
        /// Validates that a party query parameter is either a valid UUID
        /// or matches one of the predefined keywords (e.g., "me", "all") <see cref="ParamKeywords"/>.
        /// </summary>
        /// <param name="party">The value of the party query parameter to validate.</param>
        /// <param name="paramName">The name of the query parameter (used for error reporting).</param>
        /// <returns>
        /// A validation rule that adds an error if the party value is not a valid UUID
        /// and does not match any of the predefined keywords <see cref="ParamKeywords"/>.
        /// </returns>
        public static FuncExpression Party(string party, string paramName = "party") => () =>
        {
            if (Guid.TryParse(party, out var _))
            {
                return null;
            }

            foreach (var keyword in ParamKeywords)
            {
                if (string.Equals(party, keyword, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
            }

            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new("party", $"Given party of combination of parties must be a valid UUID or one of {ParamKeywordsToString}.")]);
        };

        /// <summary>
        /// Validates the 'from' party query parameter.
        /// The value must be a valid UUID or one of the predefined keywords ("me", "all") <see cref="ParamKeywords"/>.
        /// </summary>
        /// <param name="party">The value of the 'from' query parameter to validate.</param>
        /// <returns>A validation rule that adds an error if validation fails.</returns>
        public static FuncExpression PartyFrom(string party) => Party(party, "from");

        /// <summary>
        /// Validates the 'to' party query parameter.
        /// The value must be a valid UUID or one of the predefined keywords ("me", "all") <see cref="ParamKeywords"/>.
        /// </summary>
        /// <param name="party">The value of the 'to' query parameter to validate.</param>
        /// <returns>A validation rule that adds an error if validation fails.</returns>
        public static FuncExpression PartyTo(string party) => Party(party, "to");
    }
}
