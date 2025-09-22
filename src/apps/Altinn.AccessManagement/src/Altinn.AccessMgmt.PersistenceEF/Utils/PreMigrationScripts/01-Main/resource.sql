alter table dbo.resource alter column audit_changedby drop not null;\
alter table dbo.resource alter column audit_changedbysystem drop not null;
alter table dbo.resource alter column audit_changeoperation drop not null;
alter table dbo.resource alter column audit_validfrom drop default;
alter table dbo.resource alter column refid drop not null;

alter table dbo.resource drop constraint uc_resource_providerid_refid;
ALTER TABLE dbo.resource RENAME CONSTRAINT fk_resource_provider_provider TO fk_resource_provider_providerid;
ALTER TABLE dbo.resource RENAME CONSTRAINT fk_resource_type_resourcetype TO fk_resource_resourcetype_typeid;

ALTER INDEX dbo.fk_resource_providerid_provider_idx RENAME TO ix_resource_providerid;
ALTER INDEX dbo.fk_resource_typeid_resourcetype_idx RENAME TO ix_resource_typeid;
drop index dbo.uc_resource_providerid_refid_idx;

ALTER TRIGGER resource_audit_update ON dbo.resource RENAME TO audit_resource_update_trg;
ALTER TRIGGER resource_audit_delete ON dbo.resource RENAME TO audit_resource_delete_trg;
drop trigger resource_meta on dbo.resource;

create or replace function dbo.audit_resource_insert_fn() returns trigger
    language plpgsql
as
$$
BEGIN
NEW.audit_validfrom := now();
RETURN NEW;
END;
$$;

create or replace trigger audit_resource_insert_trg
    before insert or update
    on dbo.resource
    for each row
execute procedure dbo.audit_resource_insert_fn();

create or replace function dbo.audit_resource_update_fn() returns trigger
    language plpgsql
as
$$
BEGIN
INSERT INTO dbo_history.auditresource (
id,description,name,providerid,refid,typeid,
audit_validfrom, audit_validto,
audit_changedby, audit_changedbysystem, audit_changeoperation
) VALUES (
OLD.id,OLD.description,OLD.name,OLD.providerid,OLD.refid,OLD.typeid,
OLD.audit_validfrom, now(),
OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation
);
RETURN NEW;
END;
$$;

create or replace function dbo.audit_resource_delete_fn() returns trigger
    language plpgsql
as
$$
DECLARE ctx RECORD;
BEGIN
SELECT * INTO ctx FROM session_audit_context LIMIT 1;
INSERT INTO dbo_history.auditresource (
id,description,name,providerid,refid,typeid,
audit_validfrom, audit_validto,
audit_changedby, audit_changedbysystem, audit_changeoperation,
audit_deletedby, audit_deletedbysystem, audit_deleteoperation
) VALUES (
OLD.id,OLD.description,OLD.name,OLD.providerid,OLD.refid,OLD.typeid,
OLD.audit_validfrom, now(),
OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation,
ctx.changed_by, ctx.changed_by_system, ctx.change_operation_id
);
RETURN OLD;
END;
$$;

