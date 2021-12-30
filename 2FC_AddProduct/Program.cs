using System;

namespace _2FC_AddProduct
{
    // Сервис хранения информации о продукте
    class ProductInfoService
    {
        // добавляем продукта в сервис
        public int Add(Product product) { return 0; }
        // успешной загрузки изображения добавляем информацию, о том, что оно загружено
        public int EditProduct(int id, bool imageIsLoaded) { return 0; }
        // всли изображение не загружено, нужно удалить продукт из сервиса
        public void Remove(int product_id) {}
    }
    // Сервис хранения изображений
    class ImageStoreService
    {
        public void Add(byte[] image, string url) {}
        public void Remove(string url) {}
    }
    // Добавляемый продукт
    class Product
    {
        public string title;
        public string info;
        public int price;
        public byte[] image;
        public string image_url;
    }
    // Добавления продукта в сервис
    class AddProductTransaction
    {
        Product _product;
        ProductInfoService _productInfoService;
        ImageStoreService _imageStoreService;

        public AddProductTransaction(Product product, ProductInfoService productInfoService, ImageStoreService imageStoreService)
        {
            _product = product;
            _productInfoService = productInfoService;
            _imageStoreService = imageStoreService;
        }

        private int product_id;
        // Выполняет добавление продукта в разные сервисы
        public void Prepare()
        {
            // Добавляем информацию в сервис
            product_id = _productInfoService.Add(_product);
            // Загружаем изображение
            _imageStoreService.Add(_product.image, _product.image_url);
        }

        public void Commit()
        {
            // Ставим отметку, что изображение загружено
            _productInfoService.EditProduct(product_id, true);
        }
        public void RollBack()
        {
            //Удаляем информацию из сервиса / загруженное изображение в случае ошибки / таймаута
            _productInfoService.Remove(product_id);
            _imageStoreService.Remove(_product.image_url);
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
                image = new byte[] { 1 },
                image_url = Guid.NewGuid().ToString()
            },
            new ProductInfoService(), new ImageStoreService()
            );

            var TransactionManager = new TransactionManager(transaction);
            TransactionManager.Start();
        }
    }
}
