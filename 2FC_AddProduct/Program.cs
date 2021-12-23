using System;

namespace _2FC_AddProduct
{
    // Сервис хранения информации о продукте
    class ProductInfoService
    {
        public int Add(Product product) { return 0; }
        public void Remove(Product product) {}
    }
    // Сервис хранения изображений
    class ImageStoreService
    {
        public string Add(byte[] image) { return ""; }
        public void Remove(byte[] image) {}
    }
    // Сервис БД магазина
    class StoreDataBase
    {
        public void Add(StoreItem storeItem) { }
    }
    // id продукта и url изображения
    class StoreItem
    {
        public int product_id;
        public string image_url;

        public StoreItem(int product_id, string image_url)
        {
            this.product_id = product_id;
            this.image_url = image_url;
        }
    }
    // Добавляемый продукт
    class Product
    {
        public string title;
        public string info;
        public int price;
        public byte[] image;
    }
    // Добавления продукта в БД
    class AddProductTransaction
    {
        Product _product;
        ProductInfoService _productInfoService;
        ImageStoreService _imageStoreService;
        StoreDataBase _storeDataBase;

        public StoreItem storeItem;
        public AddProductTransaction(Product product, ProductInfoService productInfoService, ImageStoreService imageStoreService, StoreDataBase storeDataBase)
        {
            _product = product;
            _productInfoService = productInfoService;
            _imageStoreService = imageStoreService;
            _storeDataBase = storeDataBase;
        }
        // Выполняет добавление продукта в разные сервисы
        public void Prepare()
        {
            try
            {
                // Добавляем информацию в БД
                int product_id = _productInfoService.Add(_product);
                // Загружаем изображение
                string image_url = _imageStoreService.Add(_product.image);

                storeItem = new StoreItem(product_id, image_url);
            }
            catch (Exception e)
            {
                // Предполагается, что _productInfoService и _imageStoreService выбрасывают исключение в случае ошибки или таймаута
            }
        }
        public void Commit()
        {
            // Добавляем продукт в основную БД, в случае если добавилась информация и изображение
            _storeDataBase.Add(storeItem);
        }
        public void RollBack()
        {
            //Удаляем информацию из БД / загруженное изображение в случае ошибки / таймаута
            _productInfoService.Remove(_product);
            _imageStoreService.Remove(_product.image);
        }
    }

    // управляет процессом добавления продуктов
    class TransactionManager
    {
        AddProductTransaction _transaction;
        public TransactionManager(AddProductTransaction addProductTransaction)
        {
            _transaction = addProductTransaction;
        }

        public void Start()
        {
            try
            {
                _transaction.Prepare();
                _transaction.Commit();
            }
            catch (Exception e)
            {
                _transaction.RollBack();
                // запускаем транзакцию заного / или записываем неудачные транзации для повторения
                Start();
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var transaction = new AddProductTransaction(new Product
            {
                title = "Product",
                info = "description",
                price = 1000,
                image = new byte[] { 1 }
            },
            new ProductInfoService(), new ImageStoreService(), new StoreDataBase()
            );

            var TransactionManager = new TransactionManager(transaction);
            TransactionManager.Start();
        }
    }
}
