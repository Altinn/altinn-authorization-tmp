-- Flytter denne ut da jeg tror den kommer til å bruke lang tid
create unique index ix_entity_name_typeid_variantid on entity (name, refid, typeid, variantid);
