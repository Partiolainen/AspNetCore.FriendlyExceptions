using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AspNetCore.FriendlyExceptions.Transforms.Interfaces;

namespace AspNetCore.FriendlyExceptions.Transforms;

public class TransformsCollectionBuilder : ITransformsMap, ITransformsCollection
{
    private readonly List<ITransform> _transforms = [];

    private TransformsCollectionBuilder()
    {
    }

    public ITransform FindTransform(Exception exception)
    {
        var handler = _transforms.FirstOrDefault(x => x.CanHandle(exception));
        return handler;
    }

    public ITransformTo<T> Map<T>() where T : Exception
    {
        var transform = new Transform<T>(this);
        return transform;
    }

    public ITransformTo<Exception> Map(Func<Exception, bool> matching)
    {
        var transform = new Transform<Exception>(this, matching);
        return transform;
    }

    public ITransformTo<Exception> MapAllOthers()
    {
        return Map<Exception>();
    }

    public ITransformsCollection Done()
    {
        return this;
    }

    public static ITransformsMap Begin()
    {
        return new TransformsCollectionBuilder();
    }


    private class Transform<T>(TransformsCollectionBuilder transformsCollectionBuilder, Func<Exception, bool> matching)
        : ITransformTo<T>, ITransform
        where T : Exception
    {
        private Func<T, string> _contentGenerator;

        public Transform(TransformsCollectionBuilder transformsCollectionBuilder)
            : this(transformsCollectionBuilder, ex => ex.GetType() == typeof(T))
        {
        }

        public string GetContent(Exception ex2)
        {
            var ex = (T) ex2;
            return _contentGenerator(ex);
        }

        public bool CanHandle<T2>(T2 ex) where T2 : Exception
        {
            var result = matching(ex);
            if (!result)
                result = matching(new Exception());
            return result;
        }

        public string ContentType { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }

        public ITransformsMap To(HttpStatusCode statusCode, string reasonPhrase, Func<T, string> contentGenerator,
            string contentType = "text/plain")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ContentType = contentType;
            _contentGenerator = contentGenerator;
            transformsCollectionBuilder._transforms.Add(this);
            return transformsCollectionBuilder;
        }
    }
}