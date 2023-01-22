using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Snappy.Shared.Services;

public abstract class BaseDynamoService<TModel> where TModel : class, new()
{
    protected IDynamoDBContext DbContext;

    protected BaseDynamoService(string tableName)
    {
        var client = new AmazonDynamoDBClient();
        Init(client, tableName);
    }

    protected BaseDynamoService(IAmazonDynamoDB client, string tableName)
    {
        Init(client, tableName);
    }

    protected void Init(IAmazonDynamoDB client, string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentNullException(nameof(tableName));
        }

        var config = new DynamoDBContextConfig
        {
            Conversion = DynamoDBEntryConversion.V2,
        };

        // TODO: It seems like maybe this should be shared instead of re-created for every instance
        AWSConfigsDynamoDB.Context.TypeMappings[typeof(TModel)] = new Amazon.Util.TypeMapping(typeof(TModel), tableName);
        DbContext = new DynamoDBContext(client, config);
    }
}
