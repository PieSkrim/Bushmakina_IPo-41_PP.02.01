-- Создаем базу данных с владельцем app
CREATE DATABASE bushmakina OWNER = app;

-- Подключаемся к созданной базе от имени пользователя app
\c bushmakina app 123456789 localhost 5432

-- Создаем схему 'mealdelivery' с владельцем app
CREATE SCHEMA IF NOT EXISTS mealdelivery
    AUTHORIZATION app;

-- Комментарий к схеме
COMMENT ON SCHEMA mealdelivery IS 'Схема для данных сервиса доставки готовых обедов';

-- Устанавливаем схему по умолчанию
SET search_path TO mealdelivery, public;


-- Статусы заказа
CREATE TYPE order_status AS ENUM (
    'pending',      -- Ожидает обработки
    'preparing',    -- Готовится
    'dispatched',   -- Передан курьеру
    'delivered',    -- Доставлен
    'cancelled'     -- Отменён
);

-- Роли пользователей
CREATE TYPE user_role AS ENUM (
    'guest',        -- Гость
    'client',       -- Авторизованный клиент
    'manager',      -- Менеджер
    'admin'         -- Администратор
);

-- Таблица ролей
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name user_role NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Категории блюд
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE categories IS 'Справочник категорий готовых блюд';
COMMENT ON COLUMN categories.name IS 'Название категории (например, "Основные блюда")';

-- Производители (ваша кухня)
CREATE TABLE manufacturers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL UNIQUE,
    address TEXT,
    phone VARCHAR(50),
    email VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE manufacturers IS 'Производители готовых блюд (кухни, цеха)';

-- Поставщики ингредиентов или готовых позиций
CREATE TABLE suppliers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL UNIQUE,
    contact_person VARCHAR(150),
    phone VARCHAR(50),
    email VARCHAR(100),
    address TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE suppliers IS 'Поставщики ингредиентов или готовых блюд';

-- Пользователи системы
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    login VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role_id INTEGER NOT NULL,
    full_name VARCHAR(150) NOT NULL,
    email VARCHAR(100),
    phone VARCHAR(50),
    is_active BOOLEAN DEFAULT true,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_roles
        FOREIGN KEY (role_id)
        REFERENCES roles(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

COMMENT ON TABLE users IS 'Пользователи системы: гости, клиенты, менеджеры, админы';

-- Готовые обеды (товары)
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    category_id INTEGER NOT NULL,
    description TEXT,
    manufacturer_id INTEGER NOT NULL,
    supplier_id INTEGER, 
    price NUMERIC(10, 2) NOT NULL CHECK (price >= 0),
    unit VARCHAR(50) NOT NULL DEFAULT 'порция',
    stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    discount_percent NUMERIC(5,2) DEFAULT 0.00 CHECK (discount_percent BETWEEN 0 AND 100),
    image_path VARCHAR(255),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_products_categories
        FOREIGN KEY (category_id)
        REFERENCES categories(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT fk_products_manufacturers
        FOREIGN KEY (manufacturer_id)
        REFERENCES manufacturers(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT fk_products_suppliers
        FOREIGN KEY (supplier_id)
        REFERENCES suppliers(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

-- Заказы клиентов
CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    total_amount NUMERIC(12, 2) NOT NULL CHECK (total_amount >= 0),
    status order_status NOT NULL DEFAULT 'pending',
    delivery_address TEXT NOT NULL,
    delivery_time TIMESTAMP, 
    payment_method VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_orders_users
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

COMMENT ON TABLE orders IS 'Заказы клиентов на готовые обеды';

-- Состав заказа (многие-ко-многим)
CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    price_at_purchase NUMERIC(10, 2) NOT NULL CHECK (price_at_purchase >= 0),
    discount_at_purchase NUMERIC(5, 2) DEFAULT 0.00,
    CONSTRAINT uk_order_items UNIQUE (order_id, product_id),
    CONSTRAINT fk_order_items_orders
        FOREIGN KEY (order_id)
        REFERENCES orders(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_order_items_products
        FOREIGN KEY (product_id)
        REFERENCES products(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

COMMENT ON TABLE order_items IS 'Детали заказа: какие блюда и в каком количестве';

CREATE INDEX idx_users_login ON users(login);
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_stock ON products(stock_quantity);
CREATE INDEX idx_products_manufacturer ON products(manufacturer_id);
CREATE INDEX idx_orders_user ON orders(user_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_date ON orders(order_date);
CREATE INDEX idx_order_items_product ON order_items(product_id);

-- Функция для обновления updated_at
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Триггеры
CREATE TRIGGER trg_users_updated
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_modified_column();

CREATE TRIGGER trg_products_updated
    BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION update_modified_column();

CREATE TRIGGER trg_orders_updated
    BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION update_modified_column();

CREATE TRIGGER trg_categories_updated
    BEFORE UPDATE ON categories
    FOR EACH ROW EXECUTE FUNCTION update_modified_column();

CREATE TRIGGER trg_manufacturers_updated
    BEFORE UPDATE ON manufacturers
    FOR EACH ROW EXECUTE FUNCTION update_modified_column();

CREATE TRIGGER trg_suppliers_updated
    BEFORE UPDATE ON suppliers
    FOR EACH ROW EXECUTE FUNCTION update_modified_column();

-- Функция: можно ли удалить продукт?
CREATE OR REPLACE FUNCTION can_delete_product(product_id INTEGER)
RETURNS BOOLEAN AS $$
DECLARE
    order_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO order_count
    FROM order_items
    WHERE product_id = can_delete_product.product_id;
    RETURN order_count = 0;
END;
$$ LANGUAGE plpgsql;

-- Функция: расчёт итоговой цены со скидкой
CREATE OR REPLACE FUNCTION calculate_final_price(base_price NUMERIC, discount_percent NUMERIC)
RETURNS NUMERIC AS $$
BEGIN
    RETURN ROUND(base_price * (1 - discount_percent / 100), 2);
END;
$$ LANGUAGE plpgsql;

-- Роли
INSERT INTO roles (name, description) VALUES
('guest', 'Гость — просмотр без авторизации'),
('client', 'Клиент — может оформлять заказы'),
('manager', 'Менеджер — управление заказами и фильтрация товаров'),
('admin', 'Администратор — полный доступ');

-- Производитель (ваша организация)
INSERT INTO manufacturers (name, address, phone, email) VALUES
('ООО «Эльбрус»', 'г. Москва, ул. Примерная, д. 1', '+7 (495) 123-45-67', 'kitchen@elbrus.ru');

-- Поставщики
INSERT INTO suppliers (name, contact_person, phone, email, address) VALUES
('Фермерское хозяйство «Зелёный луг»', 'Иванов И.И.', '+7 (900) 111-22-33', 'green@meadow.ru', 'Московская обл., д. Луговое'),
('Компания «Свежие Овощи»', 'Петрова А.С.', '+7 (900) 444-55-66', 'veggies@fresh.ru', 'г. Москва, ул. Овощная, д. 10');

-- Категории
INSERT INTO categories (name, description) VALUES
('Основные блюда', 'Горячие блюда и комплексы'),
('Салаты', 'Свежие и авторские салаты'),
('Супы', 'Первые блюда'),
('Десерты', 'Сладкие блюда'),
('Напитки', 'Соки, вода, чай');

-- Пользователи
INSERT INTO users (login, password_hash, role_id, full_name, email, phone) VALUES
('guest', 'guest', (SELECT id FROM roles WHERE name = 'guest'), 'Гость', NULL, NULL),
('client1', 'password123', (SELECT id FROM roles WHERE name = 'client'), 'Иван Петров', 'ivan@example.com', '+7 (999) 123-45-67'),
('manager1', 'password123', (SELECT id FROM roles WHERE name = 'manager'), 'Мария Сидорова', 'manager@elbrus.ru', '+7 (999) 234-56-78'),
('admin', 'admin123', (SELECT id FROM roles WHERE name = 'admin'), 'Администратор Системы', 'admin@elbrus.ru', '+7 (999) 345-67-89');

-- Продукты
INSERT INTO products (name, category_id, description, manufacturer_id, supplier_id, price, unit, stock_quantity, discount_percent, image_path) VALUES
('Куриный стейк с гречкой',
 (SELECT id FROM categories WHERE name = 'Основные блюда'),
 'Куриная грудка на гриле, гречневая каша, соус песто',
 (SELECT id FROM manufacturers WHERE name = 'ООО «Эльбрус»'),
 (SELECT id FROM suppliers WHERE name = 'Фермерское хозяйство «Зелёный луг»'),
 299.00, 'порция', 15, 0, 'images/chicken_steak.jpg'),
('Салат Цезарь',
 (SELECT id FROM categories WHERE name = 'Салаты'),
 'Свежий салат с курицей, пармезаном и гренками',
 (SELECT id FROM manufacturers WHERE name = 'ООО «Эльбрус»'),
 NULL,
 199.00, 'порция', 0, 20, 'images/caesar_salad.jpg'),
('Томатный суп-пюре',
 (SELECT id FROM categories WHERE name = 'Супы'),
 'Густой суп из свежих томатов с базиликом',
 (SELECT id FROM manufacturers WHERE name = 'ООО «Эльбрус»'),
 (SELECT id FROM suppliers WHERE name = 'Компания «Свежие Овощи»'),
 149.00, 'порция', 8, 0, 'images/tomato_soup.jpg');

-- Заказ
INSERT INTO orders (user_id, total_amount, status, delivery_address, delivery_time, payment_method) VALUES
((SELECT id FROM users WHERE login = 'client1'),
 598.00,
 'delivered',
 'г. Москва, ул. Ленина, д. 10, кв. 5',
 '2025-11-10 13:00:00',
 'Онлайн');

-- Состав заказа
INSERT INTO order_items (order_id, product_id, quantity, price_at_purchase, discount_at_purchase) VALUES
((SELECT id FROM orders WHERE user_id = (SELECT id FROM users WHERE login = 'client1') LIMIT 1),
 (SELECT id FROM products WHERE name = 'Куриный стейк с гречкой'),
 2, 299.00, 0.00);

-- Явно устанавливаем владельца для всех таблиц
ALTER TABLE roles OWNER TO app;
ALTER TABLE categories OWNER TO app;
ALTER TABLE manufacturers OWNER TO app;
ALTER TABLE suppliers OWNER TO app;
ALTER TABLE users OWNER TO app;
ALTER TABLE products OWNER TO app;
ALTER TABLE orders OWNER TO app;
ALTER TABLE order_items OWNER TO app;

-- Права доступа
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA mealdelivery TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA mealdelivery TO app;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA mealdelivery TO app;
GRANT USAGE ON SCHEMA mealdelivery TO app;
GRANT CREATE ON SCHEMA mealdelivery TO app;

-- Автоматические права для новых объектов
ALTER DEFAULT PRIVILEGES IN SCHEMA mealdelivery
    GRANT ALL PRIVILEGES ON TABLES TO app;
ALTER DEFAULT PRIVILEGES IN SCHEMA mealdelivery
    GRANT ALL PRIVILEGES ON SEQUENCES TO app;
ALTER DEFAULT PRIVILEGES IN SCHEMA mealdelivery
    GRANT ALL PRIVILEGES ON FUNCTIONS TO app;