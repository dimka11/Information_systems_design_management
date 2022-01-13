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
    // Транзакция добавления информации
    class AddInfoTransaction
    {
        ProductInfoService _productInfoService;
        public TransactionManager _transactionManager;
        public AddInfoTransaction(ProductInfoService productInfoService)
        {
            _productInfoService = productInfoService;
        }

        public int Add(Product _product)
        {
            int id = _productInfoService.Add(_product);
            _transactionManager.Journal.AddInfoDone = true;
            _transactionManager.Prepared();
            return id;
        }

        public void Edit(int id, bool v)
        {
            _productInfoService.EditProduct(id, v);
        }

        public void Remove(int id)
        {
            _productInfoService.Remove(id);
        }
    }
    // Транзакция добавления изображения
    class AddImageTransaction
    {
        ImageStoreService _imageStoreService;
        public TransactionManager _transactionManager;
        public AddImageTransaction(ImageStoreService imageStoreService)
        {
            _imageStoreService = imageStoreService;
        }

        public void Add(byte[] image, string url)
        {
            _imageStoreService.Add(image, url);
            _transactionManager.Journal.AddImageDone = true;
            _transactionManager.Prepared();
        }

        public void Remove(string url)
        {
            _imageStoreService.Remove(url);
        }
    }
    // Добавления продукта в сервис
    class AddProductTransaction
    {
        Product _product;
        private AddInfoTransaction _addInfoTransaction;
        private AddImageTransaction _addImageTransaction;
        public AddProductTransaction(Product product, AddInfoTransaction addInfoTransaction, AddImageTransaction addImageTransaction)
        {
            _product = product;
            _addInfoTransaction = addInfoTransaction;
            _addImageTransaction = addImageTransaction;
        }

        public TransactionManager TransactionManager;

        private int product_id;
        // Выполняет добавление продукта в разные сервисы
        public void Prepare()
        {
            _addInfoTransaction._transactionManager = TransactionManager;
            _addImageTransaction._transactionManager = TransactionManager;
            // Добавляем информацию в сервис
            product_id = _addInfoTransaction.Add(_product);

            // Загружаем изображение
            _addImageTransaction.Add(_product.image, _product.image_url);
        }

        public void Commit()
        {
            // Ставим отметку в БД, что изображение загружено
            _addInfoTransaction.Edit(product_id, true);
        }
        public void RollBack()
        {
            //Удаляем информацию из сервиса / загруженное изображение в случае ошибки / таймаута
            _addInfoTransaction.Remove(product_id);
            _addImageTransaction.Remove(_product.image_url);
        }
    }
    // Журнал для хранения инофрмации о завершении транзакции
    class AddProductTransactionJournal
    {
        public bool AddImageDone;
        public bool AddInfoDone;
    }

    // управляет процессом добавления продуктов
    class TransactionManager
    {
        AddProductTransaction _transaction;
        public AddProductTransactionJournal Journal;
        public TransactionManager(AddProductTransaction addProductTransaction)
        {
            _transaction = addProductTransaction;
            _transaction.TransactionManager = this;
            Journal = new AddProductTransactionJournal();
        }

        private int prepared_counter = 0;

        public void Prepared()
        {
            prepared_counter++;
        }

        public void Start()
        {
            _transaction.Prepare();

            if (prepared_counter != 2) // Не все транзакции вызвали Prepared;
            {
                _transaction.RollBack();
                Start();
            }
            // Проверяем журнал на наличие отметок об успешности транзакций
            if (Journal.AddInfoDone && Journal.AddImageDone)
            {
                _transaction.Commit();
            }
            else
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
            }, new AddInfoTransaction(new ProductInfoService()), new AddImageTransaction(new ImageStoreService())
            );

            var TransactionManager = new TransactionManager(transaction);
            TransactionManager.Start();
        }
    }
}
