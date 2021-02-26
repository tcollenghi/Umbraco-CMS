using System;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing
{
    [TestFixture]
    public class ContentFinderByUrlAliasTests
    {
        [Test]
        [InlineAutoMoqData("/this/is/my/alias", 1001)]
        [InlineAutoMoqData("/anotheralias", 1001)]
        [InlineAutoMoqData("/page2/alias", 10011)]
        [InlineAutoMoqData("/2ndpagealias", 10011)]
        [InlineAutoMoqData("/only/one/alias", 100111)]
        [InlineAutoMoqData("/ONLY/one/Alias", 100111)]
        [InlineAutoMoqData("/alias43", 100121)]
        public void Lookup_By_Url_Alias (
            string relativeUrl,
            int nodeMatch,
            [Frozen] IPublishedContentCache publishedContentCache,
            [Frozen] IUmbracoContextAccessor umbracoContextAccessor,
            [Frozen] IUmbracoContext umbracoContext,
            [Frozen] IVariationContextAccessor variationContextAccessor,
            IFileService fileService,
            ContentFinderByUrlAlias sut,
            IPublishedContent[] rootContents,
            IPublishedProperty urlProperty
            )
        {
            //Arrange
            var absoluteUrl = "http://localhost" + relativeUrl;

            // Setup IUmbracoContext to return an IPublishedContentCache containing our contentItem.
            Mock.Get(umbracoContextAccessor).Setup(x => x.UmbracoContext).Returns(umbracoContext);
            Mock.Get(umbracoContext).Setup(x => x.Content).Returns(publishedContentCache);
            Mock.Get(publishedContentCache).Setup(x => x.GetAtRoot(null)).Returns(rootContents);

            // Setup the contentItem to contain the nodeId and relative url as UrlAlias
            IPublishedContent contentItem = rootContents[0];
            Mock.Get(contentItem).Setup(x => x.Id).Returns(nodeMatch);
            Mock.Get(contentItem).Setup(x => x.GetProperty(Constants.Conventions.Content.UrlAlias)).Returns(urlProperty);
            Mock.Get(urlProperty).Setup(x => x.GetValue(null, null)).Returns(relativeUrl);

            // Setup IVariationContextAccessor so our empty VariationContext gets returned.
            // No variations, so we ant an empty variation context
            var variationContext = new VariationContext();
            Mock.Get(variationContextAccessor).Setup(x => x.VariationContext).Returns(variationContext);
            var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService);

            //Act
            var result = sut.TryFindContent(publishedRequestBuilder);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(publishedRequestBuilder.PublishedContent.Id, nodeMatch);
        }


        [Test]
        [InlineAutoMoqData("/this/is/my/alias", 1001)]
        [InlineAutoMoqData("/anotheralias", 1001)]
        [InlineAutoMoqData("/page2/alias", 10011)]
        [InlineAutoMoqData("/2ndpagealias", 10011)]
        [InlineAutoMoqData("/only/one/alias", 100111)]
        [InlineAutoMoqData("/ONLY/one/Alias", 100111)]
        [InlineAutoMoqData("/alias43", 100121)]
        public void Lookup_By_Url_Alias_With_Invariant_Root_Node(
            string relativeUrl,
            int nodeMatch,
            [Frozen] IPublishedContentCache publishedContentCache,
            [Frozen] IUmbracoContextAccessor umbracoContextAccessor,
            [Frozen] IUmbracoContext umbracoContext,
            [Frozen] IVariationContextAccessor variationContextAccessor,
            IFileService fileService,
            ContentFinderByUrlAlias sut,
            IPublishedContent parentContent,
            IPublishedContent[] children,
            IPublishedProperty childUrlProperty)
        {
            //Arrange
            var absoluteUrl = "http://localhost" + relativeUrl;
            var parentUrl = "/parent";
            var parentID = 2140;

            // Setup IUmbracoContext to return an IPublishedContentCache
            Mock.Get(umbracoContextAccessor).Setup(x => x.UmbracoContext).Returns(umbracoContext);
            Mock.Get(umbracoContext).Setup(x => x.Content).Returns(publishedContentCache);
            // Setup IPublishedContentCache to return the parent contentItem when requested with the ID
            Mock.Get(publishedContentCache).Setup(x => x.GetById(parentID)).Returns(parentContent);

            // Setup the root contentItem to contain the parentID and return the children for ChildrenForAllCultures
            Mock.Get(parentContent).Setup(x => x.Id).Returns(parentID);
            Mock.Get(parentContent).Setup(x => x.ChildrenForAllCultures).Returns(children);

            // Setup children with ID and relativeUrl
            IPublishedContent child = children[0];
            Mock.Get(child).Setup(x => x.Id).Returns(nodeMatch);
            Mock.Get(child).Setup(x => x.GetProperty(Constants.Conventions.Content.UrlAlias)).Returns(childUrlProperty);
            Mock.Get(childUrlProperty).Setup(x => x.GetValue(null, null)).Returns(relativeUrl);

            // Setup IVariationContextAccessor so our empty VariationContext gets returned.
            // No variations, so we ant an empty variation context
            var variationContext = new VariationContext();
            Mock.Get(variationContextAccessor).Setup(x => x.VariationContext).Returns(variationContext);
            var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService);
            // Setup domain to contain our parentID as root node.
            var domain = new DomainAndUri(new Domain(1, "test", parentID, null, false), new Uri(absoluteUrl));
            publishedRequestBuilder.SetDomain(domain);

            //Act
            var result = sut.TryFindContent(publishedRequestBuilder);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(publishedRequestBuilder.PublishedContent.Id, nodeMatch);
        }
    }
}
