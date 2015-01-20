﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HouseControl.Library.Test
{
    [TestClass]
    public class ScheduleTests
    {
        string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\ScheduleData.txt";

        [TestInitialize]
        public void Setup()
        {
            // Use current time provider as the default;
            // override in individual tests if needed.
            ScheduleHelper.TimeProvider = new CurrentTimeProvider();
        }

        [TestMethod]
        public void ScheduleItems_OnCreation_IsPopulated()
        {
            // Arrange / Act
            var schedule = new Schedule(fileName);

            // Assert
            Assert.IsTrue(schedule.Count > 0);
        }

        [TestMethod]
        public void ScheduleItems_OnCreation_AreInFuture()
        {
            // Arrange / Act
            var schedule = new Schedule(fileName);
            var currentTime = DateTime.Now;

            // Assert
            foreach(var item in schedule)
            {
                Assert.IsTrue(item.EventTime > currentTime);
            }
        }

        [TestMethod]
        public void ScheduleItems_AfterRoll_AreInFuture()
        {
            // Arrange
            var schedule = new Schedule(fileName);
            var currentTime = DateTime.Now;
            foreach(var item in schedule)
            {
                if (item.EventTime > currentTime)
                    item.EventTime = item.EventTime - TimeSpan.FromDays(7);
                Assert.IsTrue(item.EventTime < currentTime);
            }

            // Act
            schedule.RollSchedule();

            // Assert
            foreach (var item in schedule)
            {
                Assert.IsTrue(item.EventTime > currentTime);
            }
        }

        [TestMethod]
        public void OneTimeItemInPast_AfterRoll_IsRemoved()
        {
            // Arrange
            var schedule = new Schedule(fileName);
            var originalCount = schedule.Count;
            var scheduleItem = new ScheduleItem()
            {
                Device = 1,
                Command = DeviceCommands.On,
                EventTime = DateTime.Now.AddMinutes(-2),
                IsEnabled = true,
                ScheduleSet = "",
                Type = ScheduleType.Once,
            };
            schedule.Add(scheduleItem);
            var newCount = schedule.Count;
            Assert.AreEqual(originalCount+1, newCount);

            // Act
            schedule.RollSchedule();

            // Assert
            Assert.AreEqual(originalCount, schedule.Count);
        }

        [TestMethod]
        public void OneTimeItemInFuture_AfterRoll_IsStillThere()
        {
            // Arrange
            var schedule = new Schedule(fileName);
            var originalCount = schedule.Count;
            var scheduleItem = new ScheduleItem()
            {
                Device = 1,
                Command = DeviceCommands.On,
                EventTime = DateTime.Now.AddMinutes(2),
                IsEnabled = true,
                ScheduleSet = "",
                Type = ScheduleType.Once,
            };
            schedule.Add(scheduleItem);
            var newCount = schedule.Count;
            Assert.AreEqual(originalCount + 1, newCount);

            // Act
            schedule.RollSchedule();

            // Assert
            Assert.AreEqual(newCount, schedule.Count);
        }
    }
}
