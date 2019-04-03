using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechSummit.Demo
{
    [ApiController]
    public class BooksController : ControllerBase
    {
        private static List<Book> FallbackBooks = new List<Book>
        {
            new Book { Lang = "en", Title = "Wayne Gretzky's Ghost: And Other Tales from a Lifetime in Hockey", Author = "Roy MacGregor" },
            new Book { Lang = "de", Title = "Meine freie deutsche Jugend", Author = "Claudia Rusch" }
        };

        private readonly DocumentClient _documentClient;
        private readonly CosmosDbConfig _cosmosDbConfig;

        public BooksController(DocumentClient documentClient, CosmosDbConfig cosmosDbConfig)
        {
            _documentClient = documentClient;
            _cosmosDbConfig = cosmosDbConfig;
        }

        [HttpGet("/books/en")]
        public async Task<IEnumerable<Book>> GetEnglishBooks(bool noDb = false)
        {
            if (noDb) return FallbackBooks.Where(b => b.Lang == "en");

            var collection = UriFactory.CreateDocumentCollectionUri(_cosmosDbConfig.DbName, _cosmosDbConfig.CollectionName);

            var entities = await _documentClient.CreateDocumentQuery<Book>(collection,
                new FeedOptions { PartitionKey = new PartitionKey("en") }).
                ToListAsync(Response);

            return entities;
        }

        [HttpGet("/books/de")]
        public async Task<IEnumerable<Book>> GetGermanBooks(bool noDb = false)
        {
            if (noDb) return FallbackBooks.Where(b => b.Lang == "de");

            var collection = UriFactory.CreateDocumentCollectionUri(_cosmosDbConfig.DbName, _cosmosDbConfig.CollectionName);

            var entities = await _documentClient.CreateDocumentQuery<Book>(collection,
                new FeedOptions { PartitionKey = new PartitionKey("de") }).
                ToListAsync(Response);

            return entities;
        }

        [HttpPost("/books")]
        public async Task<IActionResult> PostBook(Book book)
        {
            var collection = UriFactory.CreateDocumentCollectionUri(_cosmosDbConfig.DbName, _cosmosDbConfig.CollectionName);

            var document = await _documentClient.CreateDocumentAsync(collection, book, new RequestOptions { PartitionKey = new PartitionKey(book.Lang) });
            return StatusCode(201);
        }

        [HttpGet("/heartbeat")]
        public IActionResult Heartbeat() => Ok();
    }
}
