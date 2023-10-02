DROP SCHEMA IF EXISTS "testing" CASCADE;
CREATE SCHEMA "testing";

CREATE TABLE IF NOT EXISTS "Boxes"
(
    "box_id"        uuid PRIMARY KEY,
    "weight"        integer,
    "colour"        varchar(25),
    "material"      varchar(25),
    "price"         float,
    "created_at"    timestamp,
    "dimensions_id" uuid REFERENCES "Dimensions" ("dimensions_id") ON DELETE CASCADE
);

CREATE TABLE "Dimensions"
(
    "dimensions_id" integer PRIMARY KEY,
    "length"        float,
    "width"         float,
    "height"        float
);

CREATE TABLE "Orders"
(
    "order_id"    uuid PRIMARY KEY,
    "status"      varchar(25),
    "created_at"  timestamp,
    "updated_at"  timestamp,
    "customer_id" uuid REFERENCES "Customers" ("customer_id"),
    "address_id"  uuid REFERENCES "Addresses" ("address_id")
);

CREATE TABLE "Customers"
(
    "customer_id"  uuid PRIMARY KEY,
    "email"        varchar,
    "phone_number" varchar(20),
    "first_name"   varchar(25),
    "last_name"    varchar
);

CREATE TABLE "Addresses"
(
    "address_id"            uuid PRIMARY KEY,
    "street_name"           varchar,
    "house_number"          integer,
    "house_number_addition" varchar(10),
    "city"                  varchar,
    "postal_code"           varchar(10),
    "country"               varchar
);

CREATE TABLE "Stock"
(
    "stock_id" uuid PRIMARY KEY,
    "quantity" int
);

CREATE TABLE "Box_Order_Link"
(
    PRIMARY KEY (
        "box_id",
        "order_id"
    ),
    "box_id"   uuid REFERENCES "Boxes" ("box_id"),
    "order_id" uuid REFERENCES "Orders" ("order_id")
);

CREATE TABLE "Box_Stock_Link"
(
    PRIMARY KEY (
        "box_id",
        "stock_id"
    ),
    "box_id"   uuid REFERENCES "Boxes" ("box_id"), 
    "stock_id" uuid REFERENCES "Stock" ("stock_id")
);

CREATE TABLE "Customer_Address_Link"
(
    PRIMARY KEY (
        "customer_id",
        "address_id"
    ),
    "customer_id" uuid REFERENCES "Customers" ("customer_id"),
    "address_id"  uuid REFERENCES "Addresses" ("address_id")
);