import React, { useState, useEffect } from "react";
import { loginUser, redirectToOAuthLogin, handleOAuthCallbackPost } from "../services/authService";

function LoginPage({ onLoginSuccess }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [isOAuthLoading, setIsOAuthLoading] = useState(false);

  // Handle OAuth callback when component mounts
  useEffect(() => {
    const handleOAuthFlow = async () => {
      const urlParams = new URLSearchParams(window.location.search);
      const code = urlParams.get('code');
      const state = urlParams.get('state');
      const error = urlParams.get('error');
      const errorDescription = urlParams.get('error_description');
      
      // Debug: Log all URL parameters
      console.log('All URL parameters:');
      for (const [key, value] of urlParams.entries()) {
        console.log(`  ${key}: ${value}`);
      }
      
      // Debug: Log the raw search string
      console.log('Raw URL search string:', window.location.search);
      console.log('Full URL:', window.location.href);
      
      console.log('OAuth callback detected:', { code, state, error, errorDescription });
      console.log('Current URL:', window.location.href);
      
      // Check if we have any OAuth-related parameters at all
      const hasOAuthParams = urlParams.has('code') || urlParams.has('state') || urlParams.has('error') || 
                           urlParams.has('access_token') || urlParams.has('id_token') || 
                           urlParams.has('token') || urlParams.has('authorization_code');
      
      console.log('Has OAuth parameters:', hasOAuthParams);
      
      // Handle OAuth error first
      if (error) {
        setError(`OAuth Error: ${error}${errorDescription ? ` - ${errorDescription}` : ''}`);
        window.history.replaceState({}, document.title, window.location.pathname);
        return;
      }
      
      if (code && state) {
        try {
          setIsOAuthLoading(true);
          setError("");
          
          console.log('Processing OAuth callback with code:', code.substring(0, 10) + '...', 'and state:', state);
          
          // Handle OAuth callback
          const userData = await handleOAuthCallbackPost(code, state);
          
          console.log('OAuth callback successful, user data received:', userData);
          
          // Clean up URL parameters
          window.history.replaceState({}, document.title, window.location.pathname);
          
          // Notify parent of successful login
          onLoginSuccess(userData);
        } catch (err) {
          console.error('OAuth callback error:', err);
          setError(err.message || "OAuth login failed. Please try again.");
          // Clean up URL parameters even on error
          window.history.replaceState({}, document.title, window.location.pathname);
        } finally {
          setIsOAuthLoading(false);
        }
      } else if (hasOAuthParams) {
        // We have some OAuth params but not the expected ones
        console.warn('OAuth callback detected but missing expected parameters');
        console.warn('Expected: code and state');
        console.warn('Received parameters:', Object.fromEntries(urlParams.entries()));
        setError("OAuth response missing required parameters. The GTU OAuth system may be configured differently than expected.");
        window.history.replaceState({}, document.title, window.location.pathname);
      } else if (window.location.search.includes('oauth') || window.location.search.includes('callback')) {
        // URL suggests OAuth but no recognizable parameters
        console.warn('URL suggests OAuth callback but no OAuth parameters found');
        setError("OAuth callback detected but no parameters found. Please try again.");
        window.history.replaceState({}, document.title, window.location.pathname);
      }
    };

    handleOAuthFlow();
  }, [onLoginSuccess]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Basic validation
    if (!username || !password) {
      setError("Please enter both username and password");
      return;
    }
    
    try {
      setIsLoading(true);
      setError("");
      
      // Call the auth service to login
      const userData = await loginUser({ username, password });
      
      // Notify parent of successful login
      onLoginSuccess(userData);
    } catch (err) {
      setError(err.message || "Login failed. Please check your credentials.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleOAuthLogin = async () => {
    try {
      setIsOAuthLoading(true);
      setError("");
      
      // Redirect to OAuth login
      await redirectToOAuthLogin();
    } catch (err) {
      setError(err.message || "Failed to initiate OAuth login. Please try again.");
      setIsOAuthLoading(false);
    }
  };

  return (
    <div style={{
      minHeight: "100vh",
      background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "20px",
      fontFamily: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif"
    }}>
      {/* Background Pattern */}
      <div style={{
        position: "absolute",
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.1'%3E%3Ccircle cx='7' cy='7' r='1'/%3E%3Ccircle cx='27' cy='7' r='1'/%3E%3Ccircle cx='47' cy='7' r='1'/%3E%3Ccircle cx='7' cy='27' r='1'/%3E%3Ccircle cx='27' cy='27' r='1'/%3E%3Ccircle cx='47' cy='27' r='1'/%3E%3Ccircle cx='7' cy='47' r='1'/%3E%3Ccircle cx='27' cy='47' r='1'/%3E%3Ccircle cx='47' cy='47' r='1'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
        zIndex: 1
      }} />

      {/* Login Container */}
      <div style={{
        background: "rgba(255, 255, 255, 0.95)",
        backdropFilter: "blur(10px)",
        borderRadius: "20px",
        padding: "3rem",
        width: "100%",
        maxWidth: "450px",
        boxShadow: "0 25px 50px rgba(0, 0, 0, 0.25)",
        border: "1px solid rgba(255, 255, 255, 0.2)",
        position: "relative",
        zIndex: 2
      }}>
        {/* Logo/Header */}
        <div style={{ textAlign: "center", marginBottom: "2.5rem" }}>
          <div style={{
            width: "80px",
            height: "80px",
            background: "linear-gradient(135deg, #667eea, #764ba2)",
            borderRadius: "50%",
            margin: "0 auto 1rem",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            fontSize: "2rem",
            color: "white",
            boxShadow: "0 10px 25px rgba(102, 126, 234, 0.3)"
          }}>
            🌱
          </div>
          <h1 style={{ 
            color: "#2c3e50", 
            fontSize: "2rem", 
            fontWeight: "700", 
            margin: "0 0 0.5rem 0",
            background: "linear-gradient(135deg, #667eea, #764ba2)",
            WebkitBackgroundClip: "text",
            WebkitTextFillColor: "transparent",
            backgroundClip: "text"
          }}>
            Gebze Technical University Resource Management System
          </h1>
        </div>

        {/* OAuth Loading Message */}
        {isOAuthLoading && (
          <div style={{
            padding: "1rem",
            marginBottom: "1.5rem",
            background: "linear-gradient(135deg, #4CAF50, #45a049)",
            color: "white",
            borderRadius: "12px",
            fontSize: "0.9rem",
            fontWeight: "500",
            boxShadow: "0 4px 15px rgba(76, 175, 80, 0.3)"
          }}>
            <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "0.5rem" }}>
              <div style={{
                width: "16px",
                height: "16px",
                border: "2px solid rgba(255,255,255,0.3)",
                borderTop: "2px solid white",
                borderRadius: "50%",
                animation: "spin 1s linear infinite"
              }} />
              Processing login...
            </div>
          </div>
        )}
        
        {/* Error Message */}
        {error && (
          <div style={{
            padding: "1rem",
            marginBottom: "1.5rem",
            background: "linear-gradient(135deg, #ff6b6b, #ee5a24)",
            color: "white",
            borderRadius: "12px",
            fontSize: "0.9rem",
            fontWeight: "500",
            boxShadow: "0 4px 15px rgba(255, 107, 107, 0.3)"
          }}>
            <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
              <span>⚠️</span>
              {error}
            </div>
          </div>
        )}

        {/* OAuth Login Button */}
        <button
          onClick={handleOAuthLogin}
          disabled={isOAuthLoading || isLoading}
          style={{
            width: "100%",
            padding: "1rem",
            background: isOAuthLoading || isLoading ? "#94a3b8" : "linear-gradient(135deg, #4CAF50, #45a049)",
            color: "white",
            border: "none",
            borderRadius: "12px",
            fontSize: "1rem",
            fontWeight: "600",
            cursor: isOAuthLoading || isLoading ? "not-allowed" : "pointer",
            transition: "all 0.3s ease",
            boxShadow: isOAuthLoading || isLoading ? "none" : "0 4px 15px rgba(76, 175, 80, 0.4)",
            marginBottom: "1.5rem"
          }}
          onMouseEnter={(e) => {
            if (!isOAuthLoading && !isLoading) {
              e.target.style.transform = "translateY(-2px)";
              e.target.style.boxShadow = "0 8px 25px rgba(76, 175, 80, 0.4)";
            }
          }}
          onMouseLeave={(e) => {
            if (!isOAuthLoading && !isLoading) {
              e.target.style.transform = "translateY(0)";
              e.target.style.boxShadow = "0 4px 15px rgba(76, 175, 80, 0.4)";
            }
          }}
        >
          {isOAuthLoading ? (
            <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "0.5rem" }}>
              <div style={{
                width: "20px",
                height: "20px",
                border: "2px solid rgba(255,255,255,0.3)",
                borderTop: "2px solid white",
                borderRadius: "50%",
                animation: "spin 1s linear infinite"
              }} />
              Redirecting...
            </div>
          ) : (
            <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "0.5rem" }}>
              <span>🎓</span>
              Sign in with GTU Account
            </div>
          )}
        </button>

        {/* Divider */}
        <div style={{
          display: "flex",
          alignItems: "center",
          margin: "1.5rem 0",
          color: "#6c757d",
          fontSize: "0.9rem"
        }}>
          <div style={{ flex: 1, height: "1px", background: "#e9ecef" }} />
          <span style={{ padding: "0 1rem" }}>or continue with username</span>
          <div style={{ flex: 1, height: "1px", background: "#e9ecef" }} />
        </div>
        
        {/* Login Form */}
        <form onSubmit={handleSubmit}>
          {/* Username Field */}
          <div style={{ marginBottom: "1.5rem" }}>
            <label htmlFor="username" style={{ 
              display: "block", 
              marginBottom: "0.5rem",
              color: "#2c3e50",
              fontWeight: "600",
              fontSize: "0.9rem"
            }}>
              Username
            </label>
            <div style={{ position: "relative" }}>
              <input
                type="text"
                id="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Enter your username"
                disabled={isOAuthLoading}
                style={{
                  width: "100%",
                  padding: "1rem 1rem 1rem 3rem",
                  border: "2px solid #e9ecef",
                  borderRadius: "12px",
                  fontSize: "1rem",
                  transition: "all 0.3s ease",
                  background: isOAuthLoading ? "#f8f9fa" : "rgba(255, 255, 255, 0.8)",
                  boxSizing: "border-box",
                  outline: "none",
                  opacity: isOAuthLoading ? 0.6 : 1
                }}
                onFocus={(e) => {
                  if (!isOAuthLoading) {
                    e.target.style.borderColor = "#667eea";
                    e.target.style.boxShadow = "0 0 0 3px rgba(102, 126, 234, 0.1)";
                  }
                }}
                onBlur={(e) => {
                  e.target.style.borderColor = "#e9ecef";
                  e.target.style.boxShadow = "none";
                }}
              />
              <span style={{
                position: "absolute",
                left: "1rem",
                top: "50%",
                transform: "translateY(-50%)",
                color: "#6c757d",
                fontSize: "1.1rem"
              }}>
                👤
              </span>
            </div>
          </div>
          
          {/* Password Field */}
          <div style={{ marginBottom: "2rem" }}>
            <label htmlFor="password" style={{ 
              display: "block", 
              marginBottom: "0.5rem",
              color: "#2c3e50",
              fontWeight: "600",
              fontSize: "0.9rem"
            }}>
              Password
            </label>
            <div style={{ position: "relative" }}>
              <input
                type={showPassword ? "text" : "password"}
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter your password"
                disabled={isOAuthLoading}
                style={{
                  width: "100%",
                  padding: "1rem 3rem 1rem 3rem",
                  border: "2px solid #e9ecef",
                  borderRadius: "12px",
                  fontSize: "1rem",
                  transition: "all 0.3s ease",
                  background: isOAuthLoading ? "#f8f9fa" : "rgba(255, 255, 255, 0.8)",
                  boxSizing: "border-box",
                  outline: "none",
                  opacity: isOAuthLoading ? 0.6 : 1
                }}
                onFocus={(e) => {
                  if (!isOAuthLoading) {
                    e.target.style.borderColor = "#667eea";
                    e.target.style.boxShadow = "0 0 0 3px rgba(102, 126, 234, 0.1)";
                  }
                }}
                onBlur={(e) => {
                  e.target.style.borderColor = "#e9ecef";
                  e.target.style.boxShadow = "none";
                }}
              />
              <span style={{
                position: "absolute",
                left: "1rem",
                top: "50%",
                transform: "translateY(-50%)",
                color: "#6c757d",
                fontSize: "1.1rem"
              }}>
                🔒
              </span>
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                disabled={isOAuthLoading}
                style={{
                  position: "absolute",
                  right: "1rem",
                  top: "50%",
                  transform: "translateY(-50%)",
                  background: "none",
                  border: "none",
                  color: "#6c757d",
                  cursor: isOAuthLoading ? "not-allowed" : "pointer",
                  fontSize: "1.1rem",
                  padding: "0.25rem",
                  opacity: isOAuthLoading ? 0.6 : 1
                }}
              >
                {showPassword ? "🙈" : "👁️"}
              </button>
            </div>
          </div>
          
          {/* Login Button */}
          <button
            type="submit"
            disabled={isLoading || isOAuthLoading}
            style={{
              width: "100%",
              padding: "1rem",
              background: isLoading || isOAuthLoading ? "#94a3b8" : "linear-gradient(135deg, #667eea, #764ba2)",
              color: "white",
              border: "none",
              borderRadius: "12px",
              fontSize: "1rem",
              fontWeight: "600",
              cursor: isLoading || isOAuthLoading ? "not-allowed" : "pointer",
              transition: "all 0.3s ease",
              boxShadow: isLoading || isOAuthLoading ? "none" : "0 4px 15px rgba(102, 126, 234, 0.4)"
            }}
            onMouseEnter={(e) => {
              if (!isLoading && !isOAuthLoading) {
                e.target.style.transform = "translateY(-2px)";
                e.target.style.boxShadow = "0 8px 25px rgba(102, 126, 234, 0.4)";
              }
            }}
            onMouseLeave={(e) => {
              if (!isLoading && !isOAuthLoading) {
                e.target.style.transform = "translateY(0)";
                e.target.style.boxShadow = "0 4px 15px rgba(102, 126, 234, 0.4)";
              }
            }}
          >
            {isLoading ? (
              <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "0.5rem" }}>
                <div style={{
                  width: "20px",
                  height: "20px",
                  border: "2px solid rgba(255,255,255,0.3)",
                  borderTop: "2px solid white",
                  borderRadius: "50%",
                  animation: "spin 1s linear infinite"
                }} />
                Signing in...
              </div>
            ) : (
              "Sign In"
            )}
          </button>
        </form>
      </div>

      {/* CSS Animations */}
      <style>
        {`
          @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
          }
          
          @media (max-width: 480px) {
            .login-container {
              padding: 2rem 1.5rem !important;
              margin: 1rem !important;
            }
          }
        `}
      </style>
    </div>
  );
}

export default LoginPage;