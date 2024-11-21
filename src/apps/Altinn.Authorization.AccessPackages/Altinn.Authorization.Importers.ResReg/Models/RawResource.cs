using System;
using System.Collections.Generic;

namespace Altinn.Authorization.Importers.ResReg.Models;

public class LanguageText
{
    public string En { get; set; }
    public string Nb { get; set; }
    public string Nn { get; set; }

    public override string ToString()
    {
        return Nb ?? En ?? Nn ?? "";
    }

}

public class ResourceReference
{
    public string ReferenceSource { get; set; }
    public string Reference { get; set; }
    public string ReferenceType { get; set; }
}

public class HasCompetentAuthority
{
    public LanguageText Name { get; set; }
    public string Organization { get; set; }
    public string Orgcode { get; set; }
}

public class AuthorizationReference
{
    public string Id { get; set; }
    public string Value { get; set; }
}

public class RawResource
{
    public string Identifier { get; set; }
    public LanguageText Title { get; set; }
    public LanguageText Description { get; set; }
    public LanguageText RightDescription { get; set; }
    public string Homepage { get; set; }
    public string Status { get; set; }
    public string IsPartOf { get; set; }
    public List<ResourceReference> ResourceReferences { get; set; }
    public bool Delegable { get; set; }
    public bool Visible { get; set; }
    public HasCompetentAuthority HasCompetentAuthority { get; set; }
    public string AccessListMode { get; set; }
    public bool SelfIdentifiedUserEnabled { get; set; }
    public bool EnterpriseUserEnabled { get; set; }
    public string ResourceType { get; set; }
    public List<AuthorizationReference> AuthorizationReference { get; set; }
}
