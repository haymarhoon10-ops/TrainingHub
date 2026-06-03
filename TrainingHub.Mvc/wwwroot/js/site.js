(function () {
    document.addEventListener("DOMContentLoaded", function () {
        initializeRealtimeFeatures();
    });

    async function initializeRealtimeFeatures() {
        const hubUrl = document.body.dataset.enrollmentHubUrl;
        if (!hubUrl || !window.signalR || !window.signalR.HubConnectionBuilder) {
            return;
        }

        const connection = getOrCreateRealtimeConnection(hubUrl);
        registerEnrollmentHandlers(connection);
        registerNotificationHandlers(connection);
        registerConnectionStatusHandlers(connection);

        try {
            await ensureConnectionStarted(connection);
            await subscribeToVisibleRealtimeTargets(connection);
            setNotificationFeedStatus("Live updates connected.", "success");
        } catch (error) {
            console.error("Unable to start real-time updates.", error);
            setNotificationFeedStatus("Live updates are temporarily unavailable.", "danger");
        }
    }

    function getOrCreateRealtimeConnection(hubUrl) {
        if (window.trainingHubRealtimeConnection) {
            return window.trainingHubRealtimeConnection;
        }

        const connection = new window.signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

        window.trainingHubRealtimeConnection = connection;
        return connection;
    }

    function registerEnrollmentHandlers(connection) {
        if (connection.__trainingHubEnrollmentHandlersRegistered) {
            return;
        }

        connection.on("EnrollmentCounterUpdated", function (payload) {
            const sessionId = readPayloadValue(payload, "courseSessionId", "CourseSessionId");
            if (!sessionId) {
                return;
            }

            const currentEnrollments = readPayloadValue(payload, "currentEnrollments", "CurrentEnrollments");
            const remainingSpots = readPayloadValue(payload, "remainingSpots", "RemainingSpots");

            updateEnrollmentCounter(sessionId, "[data-live-enrollment-current]", currentEnrollments);
            updateEnrollmentCounter(sessionId, "[data-live-enrollment-remaining]", remainingSpots);
        });

        connection.__trainingHubEnrollmentHandlersRegistered = true;
    }

    function registerNotificationHandlers(connection) {
        if (connection.__trainingHubNotificationHandlersRegistered) {
            return;
        }

        connection.on("NotificationCreated", function (payload) {
            upsertNotificationRow(payload);

            const title = readPayloadValue(payload, "title", "Title") || "Notification";
            setNotificationFeedStatus("New notification received: " + title, "success");
        });

        connection.__trainingHubNotificationHandlersRegistered = true;
    }

    function registerConnectionStatusHandlers(connection) {
        if (connection.__trainingHubConnectionStatusHandlersRegistered) {
            return;
        }

        connection.onreconnecting(function () {
            setNotificationFeedStatus("Reconnecting live updates...", "warning");
        });

        connection.onreconnected(async function () {
            setNotificationFeedStatus("Live updates reconnected.", "success");
            await subscribeToVisibleRealtimeTargets(connection);
        });

        connection.onclose(function () {
            setNotificationFeedStatus("Live updates disconnected.", "danger");
        });

        connection.__trainingHubConnectionStatusHandlersRegistered = true;
    }

    async function subscribeToVisibleRealtimeTargets(connection) {
        const courseSessionIds = [...new Set(
            Array.from(document.querySelectorAll("[data-live-enrollment-session-id]"), function (element) {
                return Number.parseInt(element.dataset.liveEnrollmentSessionId || "0", 10);
            }).filter(function (sessionId) {
                return sessionId > 0;
            })
        )];

        for (const sessionId of courseSessionIds) {
            await connection.invoke("SubscribeToCourseSessionAsync", sessionId);
        }

        if (document.querySelector("[data-live-notification-table-body]")) {
            await connection.invoke("SubscribeToCurrentUserNotificationsAsync");
        }
    }

    async function ensureConnectionStarted(connection) {
        if (connection.state === window.signalR.HubConnectionState.Connected) {
            return;
        }

        if (!window.trainingHubRealtimeStartPromise) {
            window.trainingHubRealtimeStartPromise = connection.start().finally(function () {
                window.trainingHubRealtimeStartPromise = null;
            });
        }

        await window.trainingHubRealtimeStartPromise;
    }

    function updateEnrollmentCounter(sessionId, selector, value) {
        if (value === undefined || value === null) {
            return;
        }

        const elements = document.querySelectorAll(selector + "[data-live-enrollment-session-id=\"" + sessionId + "\"]");
        for (const element of elements) {
            if (element.textContent !== String(value)) {
                element.textContent = value;
                flashElement(element);
            }
        }
    }

    function upsertNotificationRow(payload) {
        const tableBody = document.querySelector("[data-live-notification-table-body]");
        if (!tableBody) {
            return;
        }

        const notificationId = readPayloadValue(payload, "notificationId", "NotificationId");
        if (!notificationId) {
            return;
        }

        const existingRow = tableBody.querySelector("[data-notification-id=\"" + notificationId + "\"]");
        const row = existingRow || document.createElement("tr");

        row.setAttribute("data-notification-id", notificationId);
        row.innerHTML = buildNotificationRowMarkup(payload);

        if (!existingRow) {
            tableBody.prepend(row);
        }

        flashElement(row);
    }

    function buildNotificationRowMarkup(payload) {
        const canManageNotifications = document.querySelector("[data-live-notification-table-body]")?.dataset.notificationCanManage === "true";
        const notificationId = readPayloadValue(payload, "notificationId", "NotificationId");
        const title = escapeHtml(readPayloadValue(payload, "title", "Title") || "");
        const message = escapeHtml(readPayloadValue(payload, "message", "Message") || "");
        const type = escapeHtml(readPayloadValue(payload, "type", "Type") || "");
        const createdAt = formatDate(readPayloadValue(payload, "createdAt", "CreatedAt"));
        const isRead = readPayloadValue(payload, "isRead", "IsRead") ? "True" : "False";
        const traineeDisplay = escapeHtml(readPayloadValue(payload, "traineeDisplay", "TraineeDisplay") || "");
        const instructorDisplay = escapeHtml(readPayloadValue(payload, "instructorDisplay", "InstructorDisplay") || "");

        return [
            "<td>", title, "</td>",
            "<td>", message, "</td>",
            "<td>", type, "</td>",
            "<td>", createdAt, "</td>",
            "<td>", isRead, "</td>",
            "<td>", traineeDisplay, "</td>",
            "<td>", instructorDisplay, "</td>",
            "<td>",
            "<a href=\"/Notifications/Details/", notificationId, "\">Details</a>",
            canManageNotifications ? " | <a href=\"/Notifications/Edit/" + notificationId + "\">Edit</a> | <a href=\"/Notifications/Delete/" + notificationId + "\">Delete</a>" : "",
            "</td>"
        ].join("");
    }

    function setNotificationFeedStatus(message, level) {
        const notificationFeed = document.querySelector("[data-live-notification-feed]");
        if (!notificationFeed) {
            return;
        }

        notificationFeed.classList.remove("d-none", "alert-secondary", "alert-success", "alert-warning", "alert-danger");
        notificationFeed.classList.add("alert-" + (level || "secondary"));
        notificationFeed.textContent = message;
    }

    function formatDate(value) {
        if (!value) {
            return "";
        }

        const parsedDate = new Date(value);
        if (Number.isNaN(parsedDate.getTime())) {
            return escapeHtml(String(value));
        }

        return escapeHtml(parsedDate.toLocaleString());
    }

    function flashElement(element) {
        element.classList.remove("live-update-flash");
        void element.offsetWidth;
        element.classList.add("live-update-flash");
    }

    function readPayloadValue(payload, preferredKey, fallbackKey) {
        if (!payload) {
            return null;
        }

        if (Object.prototype.hasOwnProperty.call(payload, preferredKey)) {
            return payload[preferredKey];
        }

        if (Object.prototype.hasOwnProperty.call(payload, fallbackKey)) {
            return payload[fallbackKey];
        }

        return null;
    }

    function escapeHtml(value) {
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#39;");
    }
})();
