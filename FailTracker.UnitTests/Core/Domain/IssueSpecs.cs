using System;
using System.Linq;
using System.Threading;
using FailTracker.Core.Domain;
using NUnit.Framework;
using SpecsFor;
using Should;

namespace FailTracker.UnitTests.Core.Domain
{
	public class IssueSpecs
	{
		public class when_creating_a_new_issue : SpecsFor<Issue>
		{
			protected override void InitializeClassUnderTest()
			{
				SUT = null;
			}

			protected override void When()
			{
				var creator = User.CreateNewUser("creator@user.com", "test");
				SUT = Issue.CreateNewIssue(Project.Create("test", creator), "Testing", creator, "Test body");
			}

			[Test]
			public void then_the_issue_is_unassigned_by_default()
			{
				SUT.IsUnassigned.ShouldBeTrue();
			}

			[Test]
			public void then_the_assigned_to_property_is_blank()
			{
				SUT.AssignedTo.ShouldBeNull();
			}

			[Test]
			public void then_the_creator_is_captured()
			{
				SUT.CreatedBy.EmailAddress.ShouldEqual("creator@user.com");
			}

			[Test]
			public void then_it_sets_the_date_created()
			{
				SUT.CreatedAt.ShouldBeInRange(DateTime.Now.AddSeconds(-5), DateTime.Now);
			}

			[Test]
			public void then_the_issue_is_editable()
			{
				Assert.DoesNotThrow(() => SUT.ReassignTo(User.CreateNewUser("other@user.com", "pass")));
			}

			[Test]
			public void then_the_status_is_not_started()
			{
				SUT.Status.ShouldEqual(Status.NotStarted);
			}

			[Test]
			public void then_it_sets_the_last_modified_date()
			{
				SUT.LastChanged.ShouldEqual(SUT.CreatedAt);
			}
		}

		public class when_editing_an_issue : given.issue_is_not_being_edited
		{
			protected override void When()
			{
				//Sleep to make sure the clock actually advances after the issue is created. 
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				SUT.BeginEdit(TestUser, "Edited!");
			}

			[Test]
			public void then_it_captures_the_editing_user()
			{
				SUT.Changes.Last().EditedBy.ShouldEqual(TestUser);
			}

			[Test]
			public void then_it_stores_the_comment()
			{
				SUT.Changes.Last().Comments.ShouldEqual("Edited!");
			}

			[Test]
			public void then_it_stores_the_date_the_story_was_edited()
			{
				SUT.LastChanged.ShouldNotEqual(SUT.CreatedAt);
			}
		}

		public class when_reassigning_an_issue : given.issue_is_being_edited
		{
			private Issue _result;

			protected override void When()
			{
				_result = SUT.ReassignTo(TestUser);
			}

			[Test]
			public void then_it_reassigns_the_issue()
			{
				SUT.AssignedTo.ShouldEqual(TestUser);
			}

			[Test]
			public void then_it_returns_the_SUT()
			{
				_result.ShouldBeSameAs(SUT);
			}

			[Test]
			public void then_it_tracks_the_change()
			{
				_result.Changes.Last().IsReassigned.ShouldBeTrue();
			}
		}

		public class when_reassigning_without_an_active_edit : given.issue_is_not_being_edited
		{
			private InvalidOperationException _exception;

			protected override void When()
			{
				_exception = Assert.Throws<InvalidOperationException>(() => SUT.ReassignTo(TestUser));
			}

			[Test]
			public void then_it_throws_an_exception()
			{
				_exception.ShouldNotBeNull();
			}
		}

		public class when_changing_the_title : given.issue_is_being_edited
		{
			protected override void When()
			{
				SUT.ChangeTitleTo("Edited!");
			}

			[Test]
			public void then_it_updates_the_title()
			{
				SUT.Title.ShouldEqual("Edited!");
			}

			[Test]
			public void then_it_captures_the_title_change()
			{
				SUT.Changes.Last().IsTitleChanged.ShouldBeTrue();
			}
		}

		public class when_changing_the_title_without_an_active_edit : given.issue_is_not_being_edited
		{
			private InvalidOperationException _exception;

			protected override void When()
			{
				_exception = Assert.Throws<InvalidOperationException>(() => SUT.ChangeTitleTo("Edited!"));
			}

			[Test]
			public void then_it_throws_an_exception()
			{
				_exception.ShouldNotBeNull();
			}
		}

		public class when_changing_the_size : given.issue_is_being_edited
		{
			protected override void When()
			{
				SUT.ChangeSizeTo(PointSize.Twenty);
			}

			[Test]
			public void then_it_sets_the_point_size()
			{
				SUT.Size.ShouldEqual(PointSize.Twenty);
			}

			[Test]
			public void then_it_tracks_the_change()
			{
				SUT.Changes.Last().IsPointSizeChanged.ShouldBeTrue();
			}
		}
		
		public class when_changing_the_type : given.issue_is_being_edited
		{
			protected override void When()
			{
				SUT.ChangeTypeTo(IssueType.Chore);
			}

			[Test]
			public void then_it_updates_the_type()
			{
				SUT.Type.ShouldEqual(IssueType.Chore);
			}

			[Test]
			public void then_it_captures_the_edit()
			{
				SUT.Changes.Last().IsTypeChanged.ShouldBeTrue();
			}
		}

		public class when_changing_the_description : given.issue_is_being_edited
		{
			private Issue _result;

			protected override void When()
			{
				_result = SUT.ChangeDescriptionTo("Edited Description!");
			}

			[Test]
			public void then_it_updates_the_description()
			{
				SUT.Description.ShouldEqual("Edited Description!");
			}

			[Test]
			public void then_it_tracks_that_the_description_was_changed()
			{
				SUT.Changes.Last().IsDescriptionChanged.ShouldBeTrue();
			}

			[Test]
			public void then_it_returns_a_reference_to_itself()
			{
				_result.ShouldBeSameAs(SUT);
			}
		}

		public class when_changing_the_description_without_an_active_edit : given.issue_is_not_being_edited
		{
			private InvalidOperationException _exception;

			protected override void When()
			{
				_exception = Assert.Throws<InvalidOperationException>(() => SUT.ChangeDescriptionTo("Test"));
			}

			[Test]
			public void then_it_throws_an_exception()
			{
				_exception.ShouldNotBeNull();
			}
		}

		public class when_completing_an_issue : given.issue_is_not_being_edited
		{
			protected override void When()
			{
				SUT.Complete(TestUser, "Story is complete.");
			}

			[Test]
			public void then_it_sets_the_status_to_complete()
			{
				SUT.Status.ShouldEqual(Status.Complete);
			}

			[Test]
			public void then_it_stores_the_notes()
			{
				SUT.Changes.Last().Comments.ShouldEqual("Story is complete.");
			}

			[Test]
			public void then_it_stores_the_completer()
			{
				SUT.Changes.Last().EditedBy.ShouldEqual(TestUser);
			}

			[Test]
			public void then_the_change_shows_the_story_was_completed()
			{
				SUT.Changes.Last().Type.ShouldEqual(ChangeType.Completed);
			}
		}

		public class when_completing_an_issue_that_is_already_completed : given.issue_is_complete
		{
			private InvalidOperationException _exception;

			protected override void When()
			{
				_exception = Assert.Throws<InvalidOperationException>(() => SUT.Complete(TestUser, "Blah"));
			}

			[Test]
			public void then_it_throws_an_exception()
			{
				_exception.ShouldNotBeNull();
			}
		}

		public class when_reactivating_an_issue : given.issue_is_complete
		{
			protected override void When()
			{
				SUT.Reactivate(CreatorUser, "Issue was not fixed.");
			}

			[Test]
			public void then_it_changes_the_issues_status()
			{
				SUT.Status.ShouldEqual(Status.NotStarted);
			}

			[Test]
			public void then_it_captures_the_reactivater()
			{
				SUT.Changes.Last().EditedBy.ShouldEqual(CreatorUser);
			}

			[Test]
			public void then_it_marks_the_issue_as_reactivated()
			{
				SUT.Changes.Last().Type.ShouldEqual(ChangeType.Reactivated);
			}
		}

		public class when_reactivating_an_issue_that_is_not_complete : given.issue_is_not_being_edited
		{
			private InvalidOperationException _exception;

			protected override void When()
			{
				_exception = Assert.Throws<InvalidOperationException>(() => SUT.Reactivate(TestUser, "Blah"));
			}

			[Test]
			public void then_it_throws_an_exception()
			{
				_exception.ShouldNotBeNull();
			}
		}

		public static class given
		{
			public abstract class an_issue_has_been_created : SpecsFor<Issue>
			{
				protected User TestUser = User.CreateNewUser("test@user.com", "12345");
				protected User CreatorUser = User.CreateNewUser("creator@user.com", "blah");

				protected override void InitializeClassUnderTest()
				{
					SUT = Issue.CreateNewIssue(Project.Create("Test", CreatorUser), "My issue", CreatorUser, "Description");
				}
			}

			public abstract class issue_is_being_edited : an_issue_has_been_created
			{
				protected override void Given()
				{
					base.Given();
					SUT.BeginEdit(TestUser, "Comment!");
				}
			}

			public abstract class issue_is_not_being_edited : an_issue_has_been_created
			{
				protected override void Given()
				{
					base.Given();
					SUT.EndEdit();
				}
			}

			public abstract class issue_is_complete : issue_is_not_being_edited
			{
				protected override void Given()
				{
					base.Given();

					SUT.Complete(TestUser, "Completed by context.");
				}
			}
		}
	}	
}