using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Platform.Authorization.Models.External;

namespace Altinn.Platform.Authorization.Mappers
{
    /// <summary>
    /// Manual mappers for XACML JSON models
    /// </summary>
    public static class XacmlJsonMappers
    {
        public static XacmlJsonRequestRoot ToInternal(this XacmlJsonRequestRootExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonRequestRoot
            {
                Request = external.Request?.ToInternal()
            };
        }

        public static XacmlJsonRequest ToInternal(this XacmlJsonRequestExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonRequest
            {
                AccessSubject = external.AccessSubject?.Select(x => x.ToInternal()).ToList(),
                Action = external.Action?.Select(x => x.ToInternal()).ToList(),
                Resource = external.Resource?.Select(x => x.ToInternal()).ToList(),
                Category = external.Category?.Select(x => x.ToInternal()).ToList(),
                RecipientSubject = external.RecipientSubject?.Select(x => x.ToInternal()).ToList(),
                IntermediarySubject = external.IntermediarySubject?.Select(x => x.ToInternal()).ToList(),
                RequestingMachine = external.RequestingMachine?.Select(x => x.ToInternal()).ToList(),
                MultiRequests = external.MultiRequests?.ToInternal(),
                ReturnPolicyIdList = external.ReturnPolicyIdList,
                CombinedDecision = external.CombinedDecision,
                XPathVersion = external.XPathVersion
            };
        }

        public static XacmlJsonCategory ToInternal(this XacmlJsonCategoryExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonCategory
            {
                Id = external.Id,
                Attribute = external.Attribute?.Select(x => x.ToInternal()).ToList(),
                Content = external.Content
            };
        }

        public static XacmlJsonAttribute ToInternal(this XacmlJsonAttributeExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonAttribute
            {
                AttributeId = external.AttributeId,
                Value = external.Value,
                DataType = external.DataType,
                Issuer = external.Issuer,
                IncludeInResult = external.IncludeInResult
            };
        }

        public static XacmlJsonMultiRequests ToInternal(this XacmlJsonMultiRequestsExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonMultiRequests
            {
                RequestReference = external.RequestReference?.Select(x => x.ToInternal()).ToList()
            };
        }

        public static XacmlJsonRequestReference ToInternal(this XacmlJsonRequestReferenceExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonRequestReference
            {
                ReferenceId = external.ReferenceId
            };
        }


        public static XacmlJsonIdReference ToInternal(this XacmlJsonIdReferenceExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonIdReference
            {
                Id = external.Id,
                Version = external.Version,
                EarliestVersion = external.EarliestVersion,
                LatestVersion = external.LatestVersion
            };
        }

        public static XacmlJsonPolicyIdentifierList ToInternal(this XacmlJsonPolicyIdentifierListExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonPolicyIdentifierList
            {
                PolicyIdReference = external.PolicyIdReference?.Select(x => x.ToInternal()).ToList(),
                PolicySetIdReference = external.PolicySetIdReference?.Select(x => x.ToInternal()).ToList()
            };
        }

        public static XacmlJsonObligationOrAdvice ToInternal(this XacmlJsonObligationOrAdviceExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonObligationOrAdvice
            {
                Id = external.Id,
                AttributeAssignment = external.AttributeAssignment?.Select(x => x.ToInternal()).ToList()
            };
        }

        public static XacmlJsonAttributeAssignment ToInternal(this XacmlJsonAttributeAssignmentExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonAttributeAssignment
            {
                AttributeId = external.AttributeId,
                Value = external.Value,
                DataType = external.DataType,
                Category = external.Category,
                Issuer = external.Issuer
            };
        }

        public static XacmlJsonStatus ToInternal(this XacmlJsonStatusExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonStatus
            {
                StatusCode = external.StatusCode?.ToInternal(),
                StatusMessage = external.StatusMessage,
                StatusDetail = external.StatusDetail
            };
        }

        public static XacmlJsonStatusCode ToInternal(this XacmlJsonStatusCodeExternal external)
        {
            if (external == null)
                return null;

            return new XacmlJsonStatusCode
            {
                Value = external.Value,
                StatusCode = external.StatusCode?.ToInternal()
            };
        }

        // External to internal converters
        public static XacmlJsonResponseExternal ToExternal(this XacmlJsonResponse response)
        {
            if (response == null)
                return null;

            return new XacmlJsonResponseExternal
            {
                Response = response.Response?.Select(x => x.ToExternal()).ToList()
            };
        }

        public static XacmlJsonResultExternal ToExternal(this XacmlJsonResult result)
        {
            if (result == null)
                return null;

            return new XacmlJsonResultExternal
            {
                Decision = result.Decision,
                Status = result.Status?.ToExternal(),
                Obligations = result.Obligations?.Select(x => x.ToExternal()).ToList(),
                AssociatedAdvice = result.AssociatedAdvice?.Select(x => x.ToExternal()).ToList(),
                Category = result.Category?.Select(x => x.ToExternal()).ToList(),
                PolicyIdentifierList = result.PolicyIdentifierList?.ToExternal()
            };
        }

        public static XacmlJsonStatusExternal ToExternal(this XacmlJsonStatus status)
        {
            if (status == null)
                return null;

            return new XacmlJsonStatusExternal
            {
                StatusCode = status.StatusCode?.ToExternal(),
                StatusMessage = status.StatusMessage,
                StatusDetail = status.StatusDetail
            };
        }

        public static XacmlJsonStatusCodeExternal ToExternal(this XacmlJsonStatusCode statusCode)
        {
            if (statusCode == null)
                return null;

            return new XacmlJsonStatusCodeExternal
            {
                Value = statusCode.Value,
                StatusCode = statusCode.StatusCode?.ToExternal()
            };
        }

        public static XacmlJsonObligationOrAdviceExternal ToExternal(this XacmlJsonObligationOrAdvice obligation)
        {
            if (obligation == null)
                return null;

            return new XacmlJsonObligationOrAdviceExternal
            {
                Id = obligation.Id,
                AttributeAssignment = obligation.AttributeAssignment?.Select(x => x.ToExternal()).ToList()
            };
        }

        public static XacmlJsonCategoryExternal ToExternal(this XacmlJsonCategory category)
        {
            if (category == null)
                return null;

            return new XacmlJsonCategoryExternal
            {
                Id = category.Id,
                Attribute = category.Attribute?.Select(x => x.ToExternal()).ToList(),
                Content = category.Content
            };
        }

        public static XacmlJsonAttributeExternal ToExternal(this XacmlJsonAttribute attribute)
        {
            if (attribute == null)
                return null;

            return new XacmlJsonAttributeExternal
            {
                AttributeId = attribute.AttributeId,
                Value = attribute.Value,
                DataType = attribute.DataType,
                Issuer = attribute.Issuer,
                IncludeInResult = attribute.IncludeInResult
            };
        }

        public static XacmlJsonPolicyIdentifierListExternal ToExternal(this XacmlJsonPolicyIdentifierList policyIdentifierList)
        {
            if (policyIdentifierList == null)
                return null;

            return new XacmlJsonPolicyIdentifierListExternal
            {
                PolicyIdReference = policyIdentifierList.PolicyIdReference?.Select(x => x.ToExternal()).ToList(),
                PolicySetIdReference = policyIdentifierList.PolicySetIdReference?.Select(x => x.ToExternal()).ToList()
            };
        }

        public static XacmlJsonIdReferenceExternal ToExternal(this XacmlJsonIdReference idReference)
        {
            if (idReference == null)
                return null;

            return new XacmlJsonIdReferenceExternal
            {
                Id = idReference.Id,
                Version = idReference.Version,
                EarliestVersion = idReference.EarliestVersion,
                LatestVersion = idReference.LatestVersion
            };
        }

        public static XacmlJsonAttributeAssignmentExternal ToExternal(this XacmlJsonAttributeAssignment attributeAssignment)
        {
            if (attributeAssignment == null)
                return null;

            return new XacmlJsonAttributeAssignmentExternal
            {
                AttributeId = attributeAssignment.AttributeId,
                Value = attributeAssignment.Value,
                DataType = attributeAssignment.DataType,
                Category = attributeAssignment.Category,
                Issuer = attributeAssignment.Issuer
            };
        }
    }
}