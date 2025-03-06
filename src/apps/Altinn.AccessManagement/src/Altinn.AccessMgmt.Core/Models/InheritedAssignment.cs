﻿namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Inherited Assignment
/// This is a view mixing data from the Assignment, Role and RoleMap tables
/// Throgugh the RoleMap table we can see the inherited roles
/// </summary>
public class InheritedAssignment
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// FromRoleId
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// FromId
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    public Guid? ViaRoleId { get; set; }

    /// <summary>
    /// Indicates if the role is inherited or directly assigned
    /// </summary>
    public string Type { get; set; }
}
public class ExtInheritedAssignment : InheritedAssignment 
{
    /// <summary>
    /// From (Entity)
    /// </summary>
    public ExtEntity From { get; set; }

    /// <summary>
    /// To (Entity)
    /// </summary>
    public ExtEntity To { get; set; }

    /// <summary>
    /// This role is assigned to the entity
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// The base role that gives the entity the role
    /// </summary>
    public Role? ViaRole { get; set; }
}
