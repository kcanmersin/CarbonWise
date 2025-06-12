import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import { 
  getAllUsers, 
  promoteUser, 
  demoteUser, 
  adminDashboardStats,
  getUsersByRole
} from "../services/adminService";
import { getCurrentUserInfo } from "../services/userService";

export default function AdminUserManagement() {
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [allUsers, setAllUsers] = useState([]); // Store all users for search
  const [stats, setStats] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionLoading, setActionLoading] = useState({});
  const [searchTerm, setSearchTerm] = useState("");
  const [filterRole, setFilterRole] = useState("All");
  const [isFilterLoading, setIsFilterLoading] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);

  // Check if current user is superuser
  useEffect(() => {
    const checkUserPermissions = async () => {
      try {
        const userData = await getCurrentUserInfo();
        setCurrentUser(userData);

        console.log("Current user data:", userData);
        
        if (!userData || userData.role !== "SuperUser") {
          // Redirect non-superusers
          console.warn("Access denied: User is not a SuperUser");
          alert("Access denied: You must be a SuperUser to access this page.");
          navigate("/");
          return;
        }
      } catch (error) {
        console.error("Error fetching current user info:", error);
        alert("Error loading user information. Please try logging in again.");
        navigate("/");
      }
    };

    checkUserPermissions();
  }, [navigate]);

  // Fetch users and stats
  useEffect(() => {
    const fetchData = async () => {
      if (!currentUser || currentUser.role !== "SuperUser") return;
      
      setIsLoading(true);
      setError(null);
      
      try {
        const [usersData, statsData] = await Promise.all([
          getAllUsers(),
          adminDashboardStats()
        ]);

        console.log("Fetched users:", usersData);
        
        setUsers(usersData);
        setAllUsers(usersData); // Store all users for search functionality
        setStats(statsData);
      } catch (error) {
        console.error("Error fetching admin data:", error);
        setError(error.message);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [currentUser]);

  // Handle role filter change
  useEffect(() => {
    const applyRoleFilter = async () => {
      if (!currentUser || currentUser.role !== "SuperUser") return;
      if (filterRole === "All") {
        setUsers(allUsers);
        return;
      }

      const roleMap = {
          "User": 0,
          "Admin": 1, 
          "SuperUser": 2
        };

      setIsFilterLoading(true);
      try {
        // Map role names to integers based on your API
        const roleId = roleMap[filterRole];
        if (roleId !== undefined) {
          // Use the GET method to filter users by role
          const filteredUsers = await getUsersByRole(roleId);
          setUsers(filteredUsers);
        }
      } catch (error) {
        console.error("Error filtering users by role:", error);
        // Fallback to client-side filtering if API fails
        const filtered = allUsers.filter(user => {
          // Convert role names to numbers for comparison
          const userRoleNum = user.role === "SuperUser" ? 2 : user.role === "Admin" ? 1 : 0;
          const filterRoleNum = roleMap[filterRole];
          return userRoleNum === filterRoleNum;
        });
        setUsers(filtered);
      } finally {
        setIsFilterLoading(false);
      }
    };

    applyRoleFilter();
  }, [filterRole, allUsers, currentUser]);

  // Handle promote user
  const handlePromote = async (userId) => {
    // Prevent multiple actions on same user
    if (actionLoading[userId]) return;

    setActionLoading(prev => ({ ...prev, [userId]: true }));
    
    try {
      const result = await promoteUser(userId);
      console.log("Promote result:", result);
      
      // Refresh users list to get updated data
      const updatedUsers = await getAllUsers();
      setUsers(updatedUsers);
      setAllUsers(updatedUsers); // Update both lists
      setAllUsers(updatedUsers); // Update both lists
      
      alert("User promoted successfully!");
    } catch (error) {
      console.error("Error promoting user:", error);
      alert(`Failed to promote user: ${error.message}`);
    } finally {
      setActionLoading(prev => ({ ...prev, [userId]: false }));
    }
  };

  // Handle demote user
  const handleDemote = async (userId) => {
    // Prevent self-demotion from Superuser
    if (currentUser && currentUser.id === userId) {
      alert("You cannot demote yourself!");
      return;
    }

    // Prevent multiple actions on same user
    if (actionLoading[userId]) return;

    setActionLoading(prev => ({ ...prev, [userId]: true }));
    
    try {
      const result = await demoteUser(userId);
      console.log("Demote result:", result);
      
      // Refresh users list to get updated data
      const updatedUsers = await getAllUsers();
      setUsers(updatedUsers);
      
      alert("User demoted successfully!");
    } catch (error) {
      console.error("Error demoting user:", error);
      alert(`Failed to demote user: ${error.message}`);
    } finally {
      setActionLoading(prev => ({ ...prev, [userId]: false }));
    }
  };

  // Filter users based on search term only (role filtering is handled by API)
  const filteredUsers = users.filter(user => {
    if (!searchTerm) return true;
    
    const searchLower = searchTerm.toLowerCase();
    return (
      user.username?.toLowerCase().includes(searchLower) ||
      user.email?.toLowerCase().includes(searchLower) ||
      user.name?.toLowerCase().includes(searchLower) ||
      user.surname?.toLowerCase().includes(searchLower)
    );
  });

  // Get role badge color
  const getRoleBadgeColor = (role) => {
    switch (role) {
      case "SuperUser":
        return { bg: "#e74c3c", color: "#fff" };
      case "Admin":
        return { bg: "#f39c12", color: "#fff" };
      case "User":
        return { bg: "#27ae60", color: "#fff" };
      default:
        return { bg: "#95a5a6", color: "#fff" };
    }
  };

  // Check if user can be demoted (only prevent self-demotion)
  const canDemote = (userId) => {
    const isSelf = currentUser && currentUser.id === userId;
    return !isSelf;
  };

  // Menu items for sidebar
  const menuItems = [
    { key: "dashboard", name: "Dashboard" },
    {
      key: "resource-monitoring",
      name: "Resource Monitoring",
      subItems: [
        { key: "electricity", name: "Electricity" },
        { key: "water", name: "Water" },
        { key: "paper", name: "Paper" },
        { key: "naturalGas", name: "Natural Gas" }
      ]
    },
    {
      key: "carbon-footprint",
      name: "Carbon Footprint",
      subItems: [
        { key: "test", name: "Test" },
        { key: "calculations", name: "Calculations" }
      ]
    },
    { key: "predictions", name: "Predictions" },
    { key: "userManagement", name: "User Management" },
    { key: "adminTools", name: "Admin Tools" },
    { key: "reports", name: "Reports" }
  ];

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
        {/* Header */}
        <div style={{ marginBottom: "2rem" }}>
          <h1 style={{ color: "#2c3e50", marginBottom: "0.5rem" }}>👑 User Management</h1>
          <p style={{ color: "#7f8c8d", margin: 0 }}>
            Manage user roles and permissions across the platform
          </p>
        </div>

        {/* Stats Cards */}
        {stats && (
          <div style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
            gap: "1.5rem",
            marginBottom: "2rem"
          }}>
            <div style={{
              background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
              color: "white",
              padding: "1.5rem",
              borderRadius: "12px",
              textAlign: "center",
              boxShadow: "0 4px 15px rgba(102, 126, 234, 0.3)"
            }}>
              <div style={{ fontSize: "2.5rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
                {stats.totalUsers || allUsers.length}
              </div>
              <div style={{ fontSize: "1rem", opacity: "0.9" }}>Total Users</div>
            </div>
            
            <div style={{
              background: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
              color: "white",
              padding: "1.5rem",
              borderRadius: "12px",
              textAlign: "center",
              boxShadow: "0 4px 15px rgba(240, 147, 251, 0.3)"
            }}>
              <div style={{ fontSize: "2.5rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
                {allUsers.filter(u => u.role === 2).length}
              </div>
              <div style={{ fontSize: "1rem", opacity: "0.9" }}>Super Users</div>
            </div>
            
            <div style={{
              background: "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
              color: "white",
              padding: "1.5rem",
              borderRadius: "12px",
              textAlign: "center",
              boxShadow: "0 4px 15px rgba(79, 172, 254, 0.3)"
            }}>
              <div style={{ fontSize: "2.5rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
                {allUsers.filter(u => u.role === 1).length}
              </div>
              <div style={{ fontSize: "1rem", opacity: "0.9" }}>Admins</div>
            </div>
            
            <div style={{
              background: "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
              color: "white",
              padding: "1.5rem",
              borderRadius: "12px",
              textAlign: "center",
              boxShadow: "0 4px 15px rgba(67, 233, 123, 0.3)"
            }}>
              <div style={{ fontSize: "2.5rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
                {allUsers.filter(u => u.role === 0).length}
              </div>
              <div style={{ fontSize: "1rem", opacity: "0.9" }}>Regular Users</div>
            </div>
          </div>
        )}

        {/* Filters and Search */}
        <div style={{
          backgroundColor: "white",
          padding: "1.5rem",
          borderRadius: "12px",
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
          marginBottom: "2rem"
        }}>
          <div style={{
            display: "grid",
            gridTemplateColumns: "1fr auto auto",
            gap: "1rem",
            alignItems: "center"
          }}>
            <input
              type="text"
              placeholder="Search users by username, email, name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{
                padding: "0.75rem 1rem",
                border: "2px solid #e9ecef",
                borderRadius: "8px",
                fontSize: "1rem",
                outline: "none",
                transition: "border-color 0.3s ease"
              }}
              onFocus={(e) => e.target.style.borderColor = "#3498db"}
              onBlur={(e) => e.target.style.borderColor = "#e9ecef"}
            />
            
            <select
              value={filterRole}
              onChange={(e) => setFilterRole(e.target.value)}
              disabled={isFilterLoading}
              style={{
                padding: "0.75rem 1rem",
                border: "2px solid #e9ecef",
                borderRadius: "8px",
                fontSize: "1rem",
                outline: "none",
                backgroundColor: isFilterLoading ? "#f8f9fa" : "white",
                cursor: isFilterLoading ? "not-allowed" : "pointer",
                opacity: isFilterLoading ? 0.7 : 1
              }}
            >
              <option value="All">All Roles</option>
              <option value="User">User (Role 0)</option>
              <option value="Admin">Admin (Role 1)</option>
              <option value="SuperUser">SuperUser (Role 2)</option>
            </select>
            
            <div style={{
              padding: "0.75rem 1rem",
              backgroundColor: "#f8f9fa",
              borderRadius: "8px",
              fontSize: "0.9rem",
              color: "#495057",
              border: "1px solid #dee2e6",
              position: "relative"
            }}>
              {isFilterLoading && (
                <div style={{
                  position: "absolute",
                  left: "0.5rem",
                  top: "50%",
                  transform: "translateY(-50%)",
                  width: "12px",
                  height: "12px",
                  border: "2px solid #f3f3f3",
                  borderTop: "2px solid #3498db",
                  borderRadius: "50%",
                  animation: "spin 1s linear infinite"
                }}></div>
              )}
              <span style={{ marginLeft: isFilterLoading ? "1.5rem" : "0" }}>
                {filteredUsers.length} user{filteredUsers.length !== 1 ? 's' : ''} found
                {filterRole !== "All" && ` (${filterRole})`}
              </span>
            </div>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div style={{
            textAlign: "center",
            padding: "3rem",
            backgroundColor: "white",
            borderRadius: "12px",
            boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
          }}>
            <div style={{
              display: "inline-block",
              width: "50px",
              height: "50px",
              border: "5px solid #f3f3f3",
              borderTop: "5px solid #3498db",
              borderRadius: "50%",
              animation: "spin 1s linear infinite"
            }}></div>
            <style>
              {`
                @keyframes spin {
                  0% { transform: rotate(0deg); }
                  100% { transform: rotate(360deg); }
                }
              `}
            </style>
            <p style={{ marginTop: "1rem", fontSize: "1.1rem", color: "#7f8c8d" }}>
              Loading user data...
            </p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div style={{
            backgroundColor: "#fff5f5",
            border: "1px solid #feb2b2",
            color: "#c53030",
            padding: "2rem",
            borderRadius: "12px",
            textAlign: "center"
          }}>
            <div style={{ fontSize: "2rem", marginBottom: "1rem" }}>⚠️</div>
            <strong>Error:</strong> {error}
            <button 
              onClick={() => window.location.reload()} 
              style={{
                marginTop: "1rem",
                padding: "0.5rem 1rem",
                backgroundColor: "#c53030",
                color: "white",
                border: "none",
                borderRadius: "6px",
                cursor: "pointer",
                display: "block",
                margin: "1rem auto 0"
              }}
            >
              Retry
            </button>
          </div>
        )}

        {/* Users Table */}
        {!isLoading && !error && (
          <div style={{
            backgroundColor: "white",
            borderRadius: "12px",
            boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
            overflow: "hidden"
          }}>
            <div style={{
              overflowX: "auto"
            }}>
              <table style={{
                width: "100%",
                borderCollapse: "collapse"
              }}>
                <thead>
                  <tr style={{
                    backgroundColor: "#f8f9fa",
                    borderBottom: "2px solid #dee2e6"
                  }}>
                    <th style={{ padding: "1rem", textAlign: "left", color: "#495057", fontWeight: "600" }}>User</th>
                    <th style={{ padding: "1rem", textAlign: "left", color: "#495057", fontWeight: "600" }}>Email</th>
                    <th style={{ padding: "1rem", textAlign: "center", color: "#495057", fontWeight: "600" }}>Role</th>
                    <th style={{ padding: "1rem", textAlign: "center", color: "#495057", fontWeight: "600" }}>Status</th>
                    <th style={{ padding: "1rem", textAlign: "center", color: "#495057", fontWeight: "600" }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredUsers.map((user, index) => {
                    const roleBadge = getRoleBadgeColor(user.role);
                    const isCurrentUser = currentUser && currentUser.id === user.id;
                    
                    return (
                      <tr key={user.id} style={{
                        borderBottom: "1px solid #f1f3f4",
                        backgroundColor: index % 2 === 0 ? "#fff" : "#fafbfc"
                      }}>
                        <td style={{ padding: "1rem" }}>
                          <div>
                            <div style={{ fontWeight: "600", color: "#2c3e50" }}>
                              {user.name && user.surname ? `${user.name} ${user.surname}` : user.username}
                              {isCurrentUser && (
                                <span style={{
                                  marginLeft: "0.5rem",
                                  fontSize: "0.8rem",
                                  color: "#3498db",
                                  fontWeight: "normal"
                                }}>
                                  (You)
                                </span>
                              )}
                            </div>
                            <div style={{ fontSize: "0.9rem", color: "#7f8c8d" }}>
                              @{user.username}
                            </div>
                          </div>
                        </td>
                        <td style={{ padding: "1rem", color: "#495057" }}>
                          {user.email}
                        </td>
                        <td style={{ padding: "1rem", textAlign: "center" }}>
                          <span style={{
                            padding: "0.25rem 0.75rem",
                            borderRadius: "20px",
                            fontSize: "0.8rem",
                            fontWeight: "600",
                            backgroundColor: roleBadge.bg,
                            color: roleBadge.color
                          }}>
                            {user.role}
                          </span>
                        </td>
                        <td style={{ padding: "1rem", textAlign: "center" }}>
                          <span style={{
                            padding: "0.25rem 0.75rem",
                            borderRadius: "20px",
                            fontSize: "0.8rem",
                            fontWeight: "600",
                            backgroundColor: user.isInInstitution ? "#d4edda" : "#f8d7da",
                            color: user.isInInstitution ? "#155724" : "#721c24"
                          }}>
                            {user.isInInstitution ? "Active" : "Inactive"}
                          </span>
                        </td>
                        <td style={{ padding: "1rem", textAlign: "center" }}>
                          <div style={{ display: "flex", gap: "0.5rem", justifyContent: "center" }}>
                            {/* Promote Button */}
                            <button
                              onClick={() => handlePromote(user.id)}
                              disabled={actionLoading[user.id]}
                              style={{
                                padding: "0.5rem 1rem",
                                backgroundColor: "#27ae60",
                                color: "white",
                                border: "none",
                                borderRadius: "6px",
                                cursor: "pointer",
                                fontSize: "0.8rem",
                                fontWeight: "600",
                                opacity: actionLoading[user.id] ? 0.6 : 1
                              }}
                              title="Promote user"
                            >
                              {actionLoading[user.id] ? "..." : "⬆️ Promote"}
                            </button>
                            
                            {/* Demote Button */}
                            <button
                              onClick={() => handleDemote(user.id)}
                              disabled={actionLoading[user.id] || !canDemote(user.id)}
                              style={{
                                padding: "0.5rem 1rem",
                                backgroundColor: !canDemote(user.id) ? "#95a5a6" : "#e74c3c",
                                color: "white",
                                border: "none",
                                borderRadius: "6px",
                                cursor: !canDemote(user.id) ? "not-allowed" : "pointer",
                                fontSize: "0.8rem",
                                fontWeight: "600",
                                opacity: actionLoading[user.id] ? 0.6 : 1
                              }}
                              title={
                                !canDemote(user.id) ? "Cannot demote yourself" : "Demote user"
                              }
                            >
                              {actionLoading[user.id] ? "..." : "⬇️ Demote"}
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            
            {filteredUsers.length === 0 && !isLoading && (
              <div style={{
                textAlign: "center",
                padding: "3rem",
                color: "#6c757d"
              }}>
                <div style={{ fontSize: "3rem", marginBottom: "1rem" }}>👥</div>
                <h3 style={{ color: "#495057", marginBottom: "1rem" }}>No users found</h3>
                <p style={{ margin: 0 }}>
                  {searchTerm || filterRole !== "All" ? 
                    "Try adjusting your search criteria or filters." : 
                    "No users available in the system."
                  }
                </p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}