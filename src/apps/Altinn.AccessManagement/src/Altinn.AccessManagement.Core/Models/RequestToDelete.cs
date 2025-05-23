﻿using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a list of rules to delete from a single policyfile
    /// </summary>
    public class RequestToDelete : IValidatableObject
    {
        /// <summary>
        /// Gets or sets a list of unique identifier for specific rules within a policy.
        /// </summary>
        public List<string> RuleIds { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user who performed the deletion.
        /// </summary>
        [Required]
        public int DeletedByUserId { get; set; }

        /// <summary>
        /// Gets or sets a list of identifiers for the user/party performing the delegation
        /// </summary>
        public List<AttributeMatch> PerformedBy { get; set; }

        /// <summary>
        /// Gets or sets the policy to delete from
        /// </summary>
        [Required]
        public PolicyMatch PolicyMatch { get; set; }

        /// <summary>
        /// Method validating the model
        /// </summary>
        /// <param name="validationContext">The context to validate for</param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> validationResult = new List<ValidationResult>();

            if (this.DeletedByUserId == 0)
            {
                validationResult.Add(new ValidationResult("Not all RequestToDelete has a value for the user performing the delete"));
            }

            return validationResult;
        }
    }

    /// <summary>
    /// Class to wrap a list of RequestToDelete
    /// </summary>
    public class RequestToDeleteRuleList : List<RequestToDelete>, IValidatableObject
    {
        /// <summary>
        /// Method validating the model
        /// </summary>
        /// <param name="validationContext">The context to validate for</param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> validationResult = new List<ValidationResult>();

            if (this.Count < 1)
            {
                validationResult.Add(new ValidationResult("Missing rulesToDelete in body"));
                return validationResult;
            }

            if (this.Any(r => r.RuleIds == null || r.RuleIds.Count == 0))
            {
                validationResult.Add(new ValidationResult("Not all RequestToDelete has RuleId"));
                return validationResult;
            }

            try
            {
                if (this.GroupBy(x => PolicyHelper.GetAltinnAppDelegationPolicyPath(x.PolicyMatch)).Any(g => g.Count() > 1))
                {
                    validationResult.Add(new ValidationResult("Input should not contain any duplicate policies"));
                    return validationResult;
                }
            }
            catch
            {
                validationResult.Add(new ValidationResult("Not all requests to delete contains valid policy paths"));
                return validationResult;
            }

            return validationResult;
        }
    }

    /// <summary>
    /// Class to wrap a list of RequestToDelete
    /// </summary>
    public class RequestToDeletePolicyList : List<RequestToDelete>, IValidatableObject
    {
        /// <summary>
        /// Method validating the model
        /// </summary>
        /// <param name="validationContext">The context to validate for</param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> validationResult = new List<ValidationResult>();

            if (this.Count < 1)
            {
                validationResult.Add(new ValidationResult("Missing policiesToDelete in body"));
                return validationResult;
            }

            try
            {
                if (this.GroupBy(x => PolicyHelper.GetAltinnAppDelegationPolicyPath(x.PolicyMatch)).Any(g => g.Count() > 1))
                {
                    validationResult.Add(new ValidationResult("Input should not contain any duplicate policies"));
                    return validationResult;
                }
            }
            catch
            {
                validationResult.Add(new ValidationResult("Not all requests to delete contains valid policy paths"));
                return validationResult;
            }

            return validationResult;
        }
    }
}
