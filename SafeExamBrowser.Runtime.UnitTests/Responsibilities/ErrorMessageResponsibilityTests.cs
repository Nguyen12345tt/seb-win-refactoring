/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class ErrorMessageResponsibilityTests
	{
		private AppConfig appConfig;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private RuntimeContext context;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;

		private ErrorMessageResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig
			{
				BrowserLogFilePath = @"C:\Logs\Browser.log",
				ClientLogFilePath = @"C:\Logs\Client.log",
				RuntimeLogFilePath = @"C:\Logs\Runtime.log",
				ServiceLogFilePath = @"C:\Logs\Service.log"
			};
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			context = new RuntimeContext();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();

			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns<TextKey>(key => key.ToString());

			sut = new ErrorMessageResponsibility(appConfig, logger.Object, messageBox.Object, context, splashScreen.Object, text.Object);
		}

		[TestMethod]
		public void MustShowMessageBoxForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			messageBox.Verify(m => m.Show(
				It.Is<string>(message => message.Contains(TextKey.MessageBox_StartupError.ToString())),
				It.Is<string>(title => title == TextKey.MessageBox_StartupErrorTitle.ToString()),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(icon => icon == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void MustShowMessageBoxForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			messageBox.Verify(m => m.Show(
				It.Is<string>(message => message.Contains(TextKey.MessageBox_ShutdownError.ToString())),
				It.Is<string>(title => title == TextKey.MessageBox_ShutdownErrorTitle.ToString()),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(icon => icon == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void MustUseSplashScreenAsParentForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.Is<IWindow>(parent => parent == splashScreen.Object)), Times.Once);
		}

		[TestMethod]
		public void MustUseSplashScreenAsParentForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.Is<IWindow>(parent => parent == splashScreen.Object)), Times.Once);
		}

		[TestMethod]
		public void MustRetrieveLocalizedTextForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			text.Verify(t => t.Get(TextKey.MessageBox_StartupError), Times.Once);
			text.Verify(t => t.Get(TextKey.MessageBox_StartupErrorTitle), Times.Once);
		}

		[TestMethod]
		public void MustRetrieveLocalizedTextForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			text.Verify(t => t.Get(TextKey.MessageBox_ShutdownError), Times.Once);
			text.Verify(t => t.Get(TextKey.MessageBox_ShutdownErrorTitle), Times.Once);
		}

		[TestMethod]
		public void MustIgnoreUnrelatedTasks()
		{
			foreach (var task in (RuntimeTask[]) Enum.GetValues(typeof(RuntimeTask)))
			{
				if (task != RuntimeTask.ShowStartupError && task != RuntimeTask.ShowShutdownError)
				{
					sut.Assume(task);
				}
			}

			messageBox.VerifyNoOtherCalls();
		}
	}
}