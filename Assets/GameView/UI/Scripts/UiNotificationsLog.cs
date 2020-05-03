using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;
using Entities;
using UnityEngine.Serialization;
using Utilities.Events;

namespace UI.Menus
{
    public class UiNotificationsLog : UiWithScrollableItems, IPointerDownHandler
    {
        private static int _maxNotifications = 10;

        private static string _menuTitle = "NOTIFICATIONS LOG";
        private static string _newField = "NEW";

        public GameObject nofificationPrefab;

        private int _newNotificationsCount = 0;
        private string NewNotificationsCountString { get { return $"{_newNotificationsCount} {_newField}"; } }

        private GameObject[] _notifications;
        private int _notificationsCount = 0;

        public override void Awake()
        {
            base.Awake();

            _newNotificationsCount = 0;
            _notificationsCount = 0;
            _notifications = new GameObject[_maxNotifications];

            titleTextLeft.Text = _menuTitle;
            titleTextRight.Text = NewNotificationsCountString;
        }

        // reset new notification count when mouse touches this window
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            _newNotificationsCount = 0;
            titleTextRight.Text = NewNotificationsCountString;
        }

        void Start()
        {
            Test();
        }

        void Test()
        {
            string longNotification =
                "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Donec odio. " +
                "Quisque volutpat mattis eros. Nullam malesuada erat ut turpis. " +
                "Suspendisse urna nibh, viverra non, semper suscipit, posuere a, pede." +
                "Donec nec justo eget felis facilisis fermentum. Aliquam porttitor mauris " +
                "sit amet orci. Aenean dignissim pellentesque felis." +
                "Morbi in sem quis dui placerat ornare. Pellentesque odio nisi, euismod in, " +
                "pharetra a, ultricies in, diam. Sed arcu. Cras consequat." +
                "Praesent dapibus, neque id cursus faucibus, tortor neque egestas auguae, " +
                "eu vulputate magna eros eu erat. Aliquam erat volutpat. " +
                "Nam dui mi, tincidunt quis, accumsan porttitor, facilisis luctus, metus.";

            AddNewNotification();
            AddNewNotification();
            AddNewNotification(longNotification);
            AddNewNotification(longNotification);
            AddNewNotification();
            AddNewNotification(longNotification);
        }

        public void AddNewNotification(string notification = "")
        {
            _newNotificationsCount++; // TODO: fix this on interaction
            titleTextRight.Text = NewNotificationsCountString;

            // add a new notification text
            var notifObj = Instantiate(nofificationPrefab);

            // setup text fields
            var uiFieldNotification = notifObj.GetComponent<UiFieldNotification>();
            uiFieldNotification.Initialize(DateTime.Now.ToString("hh:mm:ss"), notification);

            // parent it to scrollable view
            notifObj.transform.SetParent(contentRoot.transform, false);

            // add to notifications list
            // check if need to overwrite existing notification
            if (_notifications[_notificationsCount] != null)
                Destroy(_notifications[_notificationsCount]);

            _notifications[_notificationsCount] = notifObj;

            // increment notification count
            _notificationsCount = (_notificationsCount + 1) % _maxNotifications;
        }

        public void Update()
        {
            if (UnityEngine.Random.value < 0.01f)
                AddNewNotification();
        }

        // Update is called once per frame
    }

}