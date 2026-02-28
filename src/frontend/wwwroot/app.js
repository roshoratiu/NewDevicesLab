const state = {
    currentUser: null,
    overview: null
};

const elements = {
    loginView: document.getElementById("loginView"),
    appView: document.getElementById("appView"),
    loginForm: document.getElementById("loginForm"),
    loginMessage: document.getElementById("loginMessage"),
    logoutButton: document.getElementById("logoutButton"),
    currentUserName: document.getElementById("currentUserName"),
    currentUserMeta: document.getElementById("currentUserMeta"),
    homeTitle: document.getElementById("homeTitle"),
    homeCopy: document.getElementById("homeCopy"),
    groupCount: document.getElementById("groupCount"),
    permissionCount: document.getElementById("permissionCount"),
    userGroups: document.getElementById("userGroups"),
    userPermissions: document.getElementById("userPermissions"),
    adminNavLink: document.getElementById("adminNavLink"),
    adminSection: document.getElementById("adminSection"),
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

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function selectedValues(selector) {
    return [...document.querySelectorAll(selector)]
        .filter((item) => item.checked)
        .map((item) => item.value);
}

function hasPermission(permission) {
    return state.currentUser.permissions.includes("system.full_access")
        || state.currentUser.permissions.includes(permission);
}

function showLogin() {
    elements.loginView.classList.remove("hidden");
    elements.appView.classList.add("hidden");
}

function showApp() {
    elements.loginView.classList.add("hidden");
    elements.appView.classList.remove("hidden");
}

function renderUserShell() {
    const user = state.currentUser;
    const adminAccess = hasPermission("admin.access");

    elements.currentUserName.textContent = user.username;
    elements.currentUserMeta.textContent = user.groups.join(", ");
    elements.groupCount.textContent = user.groups.length;
    elements.permissionCount.textContent = user.permissions.length;

    elements.userGroups.innerHTML = user.groups
        .map((group) => `<span class="chip">${escapeHtml(group)}</span>`)
        .join("");

    elements.userPermissions.innerHTML = user.permissions
        .map((permission) => `<span class="chip">${escapeHtml(permission)}</span>`)
        .join("");

    if (adminAccess) {
        elements.homeTitle.textContent = "Admin workspace";
        elements.homeCopy.textContent = "You have access to user, group, and permission management.";
        elements.adminNavLink.classList.remove("hidden");
        elements.adminSection.classList.remove("hidden");
    } else {
        elements.homeTitle.textContent = "User workspace";
        elements.homeCopy.textContent = "You are signed in. Your available features depend on your assigned permissions.";
        elements.adminNavLink.classList.add("hidden");
        elements.adminSection.classList.add("hidden");
    }
}

function renderUsers(users) {
    if (!users.length) {
        elements.usersList.innerHTML = '<p class="muted">No users found.</p>';
        return;
    }

    elements.usersList.innerHTML = users.map((user) => `
        <article class="stack-item">
            <h4>${escapeHtml(user.username)}</h4>
            <p>${escapeHtml(user.radboudEmail)}</p>
            <p>Student number: ${escapeHtml(user.studentNumber)}</p>
            <div class="chip-list">
                ${user.groups.map((group) => `<span class="chip">${escapeHtml(group)}</span>`).join("")}
            </div>
        </article>
    `).join("");
}

function renderGroups(groups) {
    elements.groupsList.innerHTML = groups.map((group) => `
        <article class="stack-item">
            <h4>${escapeHtml(group.name)}</h4>
            <p>${escapeHtml(group.description)}</p>
            <div class="chip-list">
                ${group.permissions.map((permission) => `<span class="chip">${escapeHtml(permission)}</span>`).join("")}
            </div>
            <p>${group.memberCount} member(s)</p>
        </article>
    `).join("");
}

function renderPermissions(permissions) {
    elements.permissionsList.innerHTML = permissions
        .map((permission) => `<span class="chip">${escapeHtml(permission.code)}</span>`)
        .join("");
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
        elements.membershipGroupOptions.innerHTML = '<p class="muted">Select a user.</p>';
        return;
    }

    const selectedNames = new Set(user.groups);
    elements.membershipGroupOptions.innerHTML = state.overview.groups.map((group) => `
        <label>
            <input type="checkbox" value="${group.id}" ${selectedNames.has(group.name) ? "checked" : ""}>
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
        elements.permissionOptions.innerHTML = '<p class="muted">Select a group.</p>';
        return;
    }

    const selectedCodes = new Set(group.permissions);
    elements.permissionOptions.innerHTML = state.overview.permissions.map((permission) => `
        <label>
            <input type="checkbox" value="${permission.id}" ${selectedCodes.has(permission.code) ? "checked" : ""}>
            <span>${escapeHtml(permission.code)}</span>
        </label>
    `).join("");
}

function renderAdminOverview() {
    if (!state.overview) {
        return;
    }

    document.querySelector('[data-stat="users"]').textContent = state.overview.users.length;
    document.querySelector('[data-stat="groups"]').textContent = state.overview.groups.length;
    document.querySelector('[data-stat="permissions"]').textContent = state.overview.permissions.length;

    renderUsers(state.overview.users);
    renderGroups(state.overview.groups);
    renderPermissions(state.overview.permissions);
    renderCreateGroupOptions(state.overview.groups);
    renderMembershipUserSelect(state.overview.users);
    renderMembershipGroupOptions();
    renderGroupPermissionSelect(state.overview.groups);
    renderPermissionOptions();
}

async function loadAdminOverview() {
    if (!hasPermission("admin.access")) {
        state.overview = null;
        return;
    }

    state.overview = await apiRequest("/api/admin/overview");
    renderAdminOverview();
}

async function bootstrap() {
    try {
        state.currentUser = await apiRequest("/api/auth/me");
        renderUserShell();
        await loadAdminOverview();
        showApp();
    } catch (error) {
        if (error.status === 401) {
            showLogin();
            return;
        }

        elements.loginMessage.textContent = error.message;
        elements.loginMessage.classList.add("error");
        showLogin();
    }
}

elements.loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.loginMessage.textContent = "Signing in...";
    elements.loginMessage.classList.remove("error");

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
        renderUserShell();
        await loadAdminOverview();
        elements.loginMessage.textContent = "";
        showApp();
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
        showLogin();
    }
});

elements.membershipUserSelect.addEventListener("change", () => {
    if (state.overview) {
        renderMembershipGroupOptions();
    }
});

elements.groupPermissionSelect.addEventListener("change", () => {
    if (state.overview) {
        renderPermissionOptions();
    }
});

elements.createUserForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.createUserMessage.textContent = "Saving...";
    elements.createUserMessage.classList.remove("error");

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
        await loadAdminOverview();
    } catch (error) {
        elements.createUserMessage.textContent = error.message;
        elements.createUserMessage.classList.add("error");
    }
});

elements.membershipForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.membershipMessage.textContent = "Saving...";
    elements.membershipMessage.classList.remove("error");

    try {
        await apiRequest(`/api/admin/users/${elements.membershipUserSelect.value}/groups`, {
            method: "PUT",
            body: JSON.stringify({
                groupIds: selectedValues('#membershipGroupOptions input[type="checkbox"]')
            })
        });

        elements.membershipMessage.textContent = "Memberships updated.";
        await loadAdminOverview();
    } catch (error) {
        elements.membershipMessage.textContent = error.message;
        elements.membershipMessage.classList.add("error");
    }
});

elements.groupPermissionForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.groupPermissionMessage.textContent = "Saving...";
    elements.groupPermissionMessage.classList.remove("error");

    try {
        await apiRequest(`/api/admin/groups/${elements.groupPermissionSelect.value}/permissions`, {
            method: "PUT",
            body: JSON.stringify({
                permissionIds: selectedValues('#permissionOptions input[type="checkbox"]')
            })
        });

        elements.groupPermissionMessage.textContent = "Permissions updated.";
        await loadAdminOverview();
    } catch (error) {
        elements.groupPermissionMessage.textContent = error.message;
        elements.groupPermissionMessage.classList.add("error");
    }
});

bootstrap();
