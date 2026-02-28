const state = {
    currentUser: null,
    overview: null
};

const elements = {
    authScreen: document.getElementById("authScreen"),
    adminShell: document.getElementById("adminShell"),
    loginForm: document.getElementById("loginForm"),
    loginMessage: document.getElementById("loginMessage"),
    logoutButton: document.getElementById("logoutButton"),
    currentUserName: document.getElementById("currentUserName"),
    currentUserMeta: document.getElementById("currentUserMeta"),
    usersList: document.getElementById("usersList"),
    groupsList: document.getElementById("groupsList"),
    permissionsList: document.getElementById("permissionsList"),
    createUserForm: document.getElementById("createUserForm"),
    createUserMessage: document.getElementById("createUserMessage"),
    createGroupOptions: document.getElementById("createGroupOptions"),
    membershipForm: document.getElementById("membershipForm"),
    membershipUserSelect: document.getElementById("membershipUserSelect"),
    membershipGroupOptions: document.getElementById("membershipGroupOptions"),
    membershipMessage: document.getElementById("membershipMessage"),
    groupPermissionForm: document.getElementById("groupPermissionForm"),
    groupPermissionSelect: document.getElementById("groupPermissionSelect"),
    permissionOptions: document.getElementById("permissionOptions"),
    groupPermissionMessage: document.getElementById("groupPermissionMessage")
};

async function bootstrap() {
    try {
        state.currentUser = await apiRequest("/api/auth/me");
        showAdmin();
        await loadOverview();
    } catch (error) {
        if (error.status === 401) {
            showAuth();
            return;
        }

        showAuth();
        elements.loginMessage.textContent = error.message;
        elements.loginMessage.classList.add("error");
    }
}

function showAuth() {
    elements.authScreen.classList.remove("hidden");
    elements.adminShell.classList.add("hidden");
}

function showAdmin() {
    elements.authScreen.classList.add("hidden");
    elements.adminShell.classList.remove("hidden");
    elements.currentUserName.textContent = state.currentUser.username;
    elements.currentUserMeta.textContent = state.currentUser.groups.join(", ");
}

async function loadOverview() {
    state.overview = await apiRequest("/api/admin/overview");
    renderOverview();
}

function renderOverview() {
    const { users, groups, permissions } = state.overview;

    document.querySelector('[data-stat="users"]').textContent = users.length;
    document.querySelector('[data-stat="groups"]').textContent = groups.length;
    document.querySelector('[data-stat="permissions"]').textContent = permissions.length;

    renderUsers(users);
    renderGroups(groups);
    renderPermissions(permissions);
    renderCreateGroupOptions(groups);
    renderMembershipUserSelect(users);
    renderMembershipGroupOptions();
    renderGroupPermissionSelect(groups);
    renderPermissionOptions();
}

function renderUsers(users) {
    if (!users.length) {
        elements.usersList.innerHTML = "<p>No users yet.</p>";
        return;
    }

    elements.usersList.innerHTML = users.map((user) => `
        <article class="mini-card">
            <h3>${escapeHtml(user.username)}</h3>
            <p>${escapeHtml(user.radboudEmail)}</p>
            <p>Student #: ${escapeHtml(user.studentNumber)}</p>
            <p>Enrolled: ${escapeHtml(user.enrollmentDate)}</p>
            <div class="chip-row">
                ${user.groups.map((group) => `<span class="chip">${escapeHtml(group)}</span>`).join("")}
            </div>
        </article>
    `).join("");
}

function renderGroups(groups) {
    elements.groupsList.innerHTML = groups.map((group) => `
        <article class="mini-card">
            <h3>${escapeHtml(group.name)}</h3>
            <p>${escapeHtml(group.description)}</p>
            <div class="chip-row">
                ${group.permissions.map((permission) => `<span class="chip">${escapeHtml(permission)}</span>`).join("")}
            </div>
            <p>${group.memberCount} member(s)</p>
        </article>
    `).join("");
}

function renderPermissions(permissions) {
    elements.permissionsList.innerHTML = permissions.map((permission) => `
        <span class="tag" title="${escapeHtml(permission.description)}">${escapeHtml(permission.code)}</span>
    `).join("");
}

function renderCreateGroupOptions(groups) {
    elements.createGroupOptions.innerHTML = groups.map((group) => `
        <label>
            <input type="checkbox" name="groupIds" value="${group.id}">
            <span>${escapeHtml(group.name)}</span>
        </label>
    `).join("");
}

function renderMembershipUserSelect(users) {
    const previous = elements.membershipUserSelect.value;

    elements.membershipUserSelect.innerHTML = users.map((user) => `
        <option value="${user.id}">${escapeHtml(user.username)} (${escapeHtml(user.radboudEmail)})</option>
    `).join("");

    if (previous && users.some((user) => user.id === previous)) {
        elements.membershipUserSelect.value = previous;
    }
}

function renderMembershipGroupOptions() {
    const user = state.overview.users.find((item) => item.id === elements.membershipUserSelect.value);

    if (!user) {
        elements.membershipGroupOptions.innerHTML = "<p>Choose a user first.</p>";
        return;
    }

    const selectedNames = new Set(user.groups);
    elements.membershipGroupOptions.innerHTML = state.overview.groups.map((group) => `
        <label>
            <input type="checkbox" name="membershipGroupIds" value="${group.id}" ${selectedNames.has(group.name) ? "checked" : ""}>
            <span>${escapeHtml(group.name)}</span>
        </label>
    `).join("");
}

function renderGroupPermissionSelect(groups) {
    const previous = elements.groupPermissionSelect.value;

    elements.groupPermissionSelect.innerHTML = groups.map((group) => `
        <option value="${group.id}">${escapeHtml(group.name)}</option>
    `).join("");

    if (previous && groups.some((group) => group.id === previous)) {
        elements.groupPermissionSelect.value = previous;
    }
}

function renderPermissionOptions() {
    const group = state.overview.groups.find((item) => item.id === elements.groupPermissionSelect.value);

    if (!group) {
        elements.permissionOptions.innerHTML = "<p>Choose a group first.</p>";
        return;
    }

    const selectedCodes = new Set(group.permissions);
    elements.permissionOptions.innerHTML = state.overview.permissions.map((permission) => `
        <label>
            <input type="checkbox" name="permissionIds" value="${permission.id}" ${selectedCodes.has(permission.code) ? "checked" : ""}>
            <span>${escapeHtml(permission.code)}</span>
        </label>
    `).join("");
}

async function apiRequest(url, options = {}) {
    const response = await fetch(url, {
        credentials: "same-origin",
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {})
        }
    });

    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers.get("content-type") || "";
    const payload = contentType.includes("application/json")
        ? await response.json()
        : await response.text();

    if (!response.ok) {
        const message = typeof payload === "string" ? payload : payload?.title || "Request failed.";
        const error = new Error(message);
        error.status = response.status;
        throw error;
    }

    return payload;
}

function selectedValues(selector) {
    return [...document.querySelectorAll(selector)]
        .filter((item) => item.checked)
        .map((item) => item.value);
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

elements.loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.loginMessage.classList.remove("error");
    elements.loginMessage.textContent = "Signing in...";

    const formData = new FormData(elements.loginForm);

    try {
        state.currentUser = await apiRequest("/api/auth/login", {
            method: "POST",
            body: JSON.stringify({
                identifier: formData.get("identifier"),
                password: formData.get("password"),
                rememberMe: formData.get("rememberMe") === "on"
            })
        });

        elements.loginForm.reset();
        showAdmin();
        await loadOverview();
        elements.loginMessage.textContent = "";
    } catch (error) {
        elements.loginMessage.textContent = error.message;
        elements.loginMessage.classList.add("error");
    }
});

elements.logoutButton.addEventListener("click", async () => {
    try {
        await apiRequest("/api/auth/logout", { method: "POST" });
    } finally {
        state.currentUser = null;
        state.overview = null;
        showAuth();
    }
});

elements.createUserForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.createUserMessage.classList.remove("error");
    elements.createUserMessage.textContent = "Saving...";

    const formData = new FormData(elements.createUserForm);

    try {
        await apiRequest("/api/admin/users", {
            method: "POST",
            body: JSON.stringify({
                radboudEmail: formData.get("radboudEmail"),
                username: formData.get("username"),
                password: formData.get("password"),
                studentNumber: formData.get("studentNumber"),
                enrollmentDate: formData.get("enrollmentDate"),
                groupIds: selectedValues('#createGroupOptions input[type="checkbox"]')
            })
        });

        elements.createUserForm.reset();
        elements.createUserMessage.textContent = "User created.";
        await loadOverview();
    } catch (error) {
        elements.createUserMessage.textContent = error.message;
        elements.createUserMessage.classList.add("error");
    }
});

elements.membershipUserSelect.addEventListener("change", () => {
    renderMembershipGroupOptions();
});

elements.membershipForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.membershipMessage.classList.remove("error");
    elements.membershipMessage.textContent = "Saving...";

    try {
        await apiRequest(`/api/admin/users/${elements.membershipUserSelect.value}/groups`, {
            method: "PUT",
            body: JSON.stringify({
                groupIds: selectedValues('#membershipGroupOptions input[type="checkbox"]')
            })
        });

        elements.membershipMessage.textContent = "Memberships updated.";
        await loadOverview();
    } catch (error) {
        elements.membershipMessage.textContent = error.message;
        elements.membershipMessage.classList.add("error");
    }
});

elements.groupPermissionSelect.addEventListener("change", () => {
    renderPermissionOptions();
});

elements.groupPermissionForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.groupPermissionMessage.classList.remove("error");
    elements.groupPermissionMessage.textContent = "Saving...";

    try {
        await apiRequest(`/api/admin/groups/${elements.groupPermissionSelect.value}/permissions`, {
            method: "PUT",
            body: JSON.stringify({
                permissionIds: selectedValues('#permissionOptions input[type="checkbox"]')
            })
        });

        elements.groupPermissionMessage.textContent = "Permissions updated.";
        await loadOverview();
    } catch (error) {
        elements.groupPermissionMessage.textContent = error.message;
        elements.groupPermissionMessage.classList.add("error");
    }
});

bootstrap();
