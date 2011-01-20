using System;
using System.Configuration;
using System.Security;
using FluentSecurity.Specification.Helpers;
using FluentSecurity.Specification.TestData;
using NUnit.Framework;

namespace FluentSecurity.Specification
{
	[TestFixture]
	[Category("SecurityHandlerSpec")]
	public class When_handling_security
	{
		[SetUp]
		public void SetUp()
		{
			SecurityConfigurator.Reset();
		}

		[Test]
		public void Should_throw_ArgumentException_when_controllername_is_null_or_empty()
		{
			// Arrange
			var securityHandler = new SecurityHandler();

			// Assert
			Assert.Throws<ArgumentException>(() => securityHandler.HandleSecurityFor(null, "A"));
			Assert.Throws<ArgumentException>(() => securityHandler.HandleSecurityFor("", "A"));
		}

		[Test]
		public void Should_throw_ArgumentException_when_actionname_is_null_or_empty()
		{
			// Arrange
			var securityHandler = new SecurityHandler();

			// Assert
			Assert.Throws<ArgumentException>(() => securityHandler.HandleSecurityFor("A", null));
			Assert.Throws<ArgumentException>(() => securityHandler.HandleSecurityFor("A", ""));
		}

		[Test]
		public void Should_not_throw_when_when_controllername_is_Blog_and_actionname_is_Index()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsTrue);
				policy.For<BlogController>(x => x.Index()).DenyAnonymousAccess();
			});

			var securityHandler = new SecurityHandler();

			// Assert
			Assert.DoesNotThrow(() => securityHandler.HandleSecurityFor("Blog", "Index"));
		}
	}

	[TestFixture]
	[Category("SecurityHandlerSpec")]
	public class When_handling_security_for_a_controlleraction_with_DenyAnonymousAccess
	{
		[SetUp]
		public void SetUp()
		{
			SecurityConfigurator.Reset();
		}

		[Test]
		public void Should_not_throw_exception_when_the_user_is_authenticated()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsTrue);
				policy.For<BlogController>(x => x.Index()).DenyAnonymousAccess();
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.DoesNotThrow(() => securityHandler.HandleSecurityFor("Blog", "Index"));
		}

		[Test]
		public void Should_throw_SecurityException_when_the_user_is_anonymous()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsFalse);
				policy.For<BlogController>(x => x.Index()).DenyAnonymousAccess();
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.Throws<SecurityException>(() => securityHandler.HandleSecurityFor("Blog", "Index"));
		}
	}

	[TestFixture]
	[Category("SecurityHandlerSpec")]
	public class When_handling_security_for_a_controlleraction_with_RequireRole_Owner
	{
		[SetUp]
		public void SetUp()
		{
			SecurityConfigurator.Reset();
		}

		[Test]
		public void Should_not_throw_exception_when_the_user_is_authenticated_with_role_Owner()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsTrue);
				policy.GetRolesFrom(StaticHelper.GetRolesIncludingOwner);
				policy.For<BlogController>(x => x.DeletePost(0)).RequireRole(UserRole.Owner);
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.DoesNotThrow(() => securityHandler.HandleSecurityFor("Blog", "DeletePost"));
		}

		[Test]
		public void Should_throw_SecurityException_when_the_user_is_anonymous()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsFalse);
				policy.GetRolesFrom(StaticHelper.GetRolesExcludingOwner);
				policy.For<BlogController>(x => x.DeletePost(0)).RequireRole(UserRole.Owner);
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.Throws<SecurityException>(() => securityHandler.HandleSecurityFor("Blog", "DeletePost"));
		}

		[Test]
		public void Should_throw_SecurityException_when_the_user_does_not_have_the_role_Owner()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsTrue);
				policy.GetRolesFrom(StaticHelper.GetRolesExcludingOwner);
				policy.For<BlogController>(x => x.DeletePost(0)).RequireRole(UserRole.Owner);
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.Throws<SecurityException>(() => securityHandler.HandleSecurityFor("Blog", "DeletePost"));
		}
	}

	[TestFixture]
	[Category("SecurityHandlerSpec")]
	public class When_handling_security_for_a_controller_and_action_that_has_no_container
	{
		[SetUp]
		public void SetUp()
		{
			SecurityConfigurator.Reset();
		}

		[Test]
		public void Should_throw_ConfigurationErrorsException_when_IgnoreMissingConfigurations_is_false()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsTrue);
				policy.For<BlogController>(x => x.Index()).DenyAnonymousAccess();
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.Throws<ConfigurationErrorsException>(() => securityHandler.HandleSecurityFor("NonConfiguredController", "Action"));
		}

		[Test]
		public void Should_not_throw_ConfigurationErrorsException_when_IgnoreMissingConfigurations_is_true()
		{
			// Arrange
			SecurityConfigurator.Configure(policy =>
			{
				policy.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsTrue);
				policy.IgnoreMissingConfiguration();
				policy.For<BlogController>(x => x.Index()).DenyAnonymousAccess();
			});

			var securityHandler = new SecurityHandler();

			// Act & Assert
			Assert.DoesNotThrow(() => securityHandler.HandleSecurityFor("NonConfiguredController", "Action"));
		}
	}
}
