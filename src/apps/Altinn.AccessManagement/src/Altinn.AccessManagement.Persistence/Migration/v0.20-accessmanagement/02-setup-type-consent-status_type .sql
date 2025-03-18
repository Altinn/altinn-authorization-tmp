-- Enum: consent.status_type

CREATE TYPE consent.status_type AS ENUM(
    'unopened', -- hva betyr dette? Trenger vi dette?
    'opened',  -- hva betyr dette? Trenger vi dette?
    'accepted',
    'rejected',
    'deleted',
    'created' -- pending? 
);
