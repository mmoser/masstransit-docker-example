using System;
using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ServiceBus.SagaInstances
{
  public class HelloWorldInstance : SagaStateMachineInstance, IVersionedSaga
  {
    public HelloWorldInstance(Guid correlationId)
    {
      CorrelationId = correlationId;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Greeting { get; set; }
  }
}
