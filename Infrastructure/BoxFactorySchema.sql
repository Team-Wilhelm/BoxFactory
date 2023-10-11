DROP SCHEMA IF EXISTS testing CASCADE;
CREATE SCHEMA testing;

CREATE TABLE IF NOT EXISTS testing.Dimensions
(
    "dimensions_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "length"        float,
    "width"         float,
    "height"        float
);

CREATE TABLE IF NOT EXISTS testing.Boxes
(
    "box_id"        uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "weight"        float,
    "colour"        varchar(25),
    "material"      varchar(25),
    "price"         float,
    "created_at"    timestamp,
    "stock"         integer,
    "dimensions_id" uuid REFERENCES testing.Dimensions ("dimensions_id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS testing.Customers
(
    "customer_email"        varchar PRIMARY KEY,
    "phone_number" varchar(20),
    "first_name"   varchar(25),
    "last_name"    varchar,
    "simpson_img_url" varchar
);

CREATE TABLE IF NOT EXISTS testing.Addresses
(
    "address_id"            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "street_name"           varchar,
    "house_number"          integer,
    "house_number_addition" varchar(10),
    "city"                  varchar,
    "postal_code"           varchar(10),
    "country"               varchar(25)
);

CREATE TABLE IF NOT EXISTS testing.Orders
(
    "order_id"    uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "status"      varchar(25),
    "created_at"  timestamp,
    "updated_at"  timestamp,
    "customer_email" varchar REFERENCES testing.Customers ("customer_email"),
    "address_id"  uuid REFERENCES testing.Addresses ("address_id")
);

CREATE TABLE IF NOT EXISTS testing.Box_Order_Link
(
    PRIMARY KEY (
        "box_id",
        "order_id"
    ),
    "box_id"   uuid REFERENCES testing.Boxes ("box_id"),
    "order_id" uuid REFERENCES testing.Orders ("order_id"),
    "quantity" integer
);

CREATE TABLE IF NOT EXISTS testing.Customer_Address_Link
(
    PRIMARY KEY (
        "customer_email",
        "address_id"
    ),
    "customer_email" varchar REFERENCES testing.Customers ("customer_email"),
    "address_id"  uuid REFERENCES testing.Addresses ("address_id")
);

CREATE TABLE IF NOT EXISTS testing.Materials
(
    "name" varchar PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS testing.Colours 
(
    "name" varchar PRIMARY KEY
);

INSERT INTO testing.Materials (name) VALUES ('cardboard'), ('plastic'), ('wood'), ('metal');
INSERT INTO testing.Colours (name) VALUES ('red'), ('blue'), ('green'), ('yellow'), ('black'), 
                                          ('white'), ('brown'), ('grey'), ('orange'), ('purple'), 
                                          ('pink'), ('gold'), ('silver'), ('bronze'), ('copper');