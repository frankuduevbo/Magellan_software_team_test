-- create
CREATE TABLE item (
  id serial PRIMARY KEY,
  item_name varchar(50) NOT NULL,
  parent_item integer,
  cost integer NOT NULL,
  req_date date NOT NULL
);

-- insert
INSERT INTO item (item_name,parent_item,cost, req_date) VALUES 
('Item1',NULL,500,'02-20-2024'),
('Sub1',1,200,'02-10-2024'),
('Sub2',1,300,'01-05-2024'),
('Sub3',2,300,'01-02-2024'),
('Sub4',2,400,'01-02-2024'),
('Item2',NULL,600,'03-15-2024'),
('Sub1',6,200,'02-25-2024');



CREATE OR REPLACE FUNCTION Get_Child_Cost(my_id integer) 
  RETURNS integer AS $return_value$
DECLARE 
  ch_count integer;
BEGIN
  SELECT count(id) INTO ch_count FROM item WHERE parent_item = my_id;
  INSERT INTO ch (ch_id, cost) 
  SELECT id, cost FROM item WHERE parent_item = my_id;
  IF ch_count > 0 THEN 
   PERFORM Get_Child_Cost(id) FROM item WHERE parent_item = my_id;
  END IF;
  RETURN 1;
END;
$return_value$ 
LANGUAGE plpgsql;



CREATE OR REPLACE FUNCTION Get_Total_Cost(name varchar) 
  RETURNS integer AS $total$
DECLARE 
  my_id integer;
  p_id integer;
  total integer;
BEGIN
  SELECT parent_item into p_id FROM item WHERE item_name = name;
  IF p_id IS NOT NULL THEN
    RETURN NULL;
  ELSE
  
    CREATE TABLE ch (
      id serial PRIMARY KEY,
      ch_id integer,
      cost integer
    );
    
    PERFORM Get_Child_Cost(id)  FROM item where item_name = name;
    
    SELECT sum(cost)  INTO total FROM ch;
    SELECT cost + total INTO total FROM item where item_name = name;
    
    DROP TABLE ch;
    RETURN total;
  END IF;
END;
$total$ 
LANGUAGE plpgsql;
