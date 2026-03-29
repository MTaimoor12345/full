import { Outlet, NavLink } from 'react-router-dom'

function Layout() {
  return (
    <div className="d-flex">
      <aside className="sidebar">
        <div className="sidebar-brand">
          SportsStore Admin
        </div>
        <nav>
          <ul className="sidebar-nav">
            <li className="sidebar-nav-item">
              <NavLink 
                to="/dashboard" 
                className={({ isActive }) => `sidebar-nav-link ${isActive ? 'active' : ''}`}
              >
                Dashboard
              </NavLink>
            </li>
            <li className="sidebar-nav-item">
              <NavLink 
                to="/orders" 
                className={({ isActive }) => `sidebar-nav-link ${isActive ? 'active' : ''}`}
              >
                Orders
              </NavLink>
            </li>
            <li className="sidebar-nav-item">
              <NavLink 
                to="/inventory" 
                className={({ isActive }) => `sidebar-nav-link ${isActive ? 'active' : ''}`}
              >
                Inventory
              </NavLink>
            </li>
            <li className="sidebar-nav-item">
              <NavLink 
                to="/payments" 
                className={({ isActive }) => `sidebar-nav-link ${isActive ? 'active' : ''}`}
              >
                Payments
              </NavLink>
            </li>
            <li className="sidebar-nav-item">
              <NavLink 
                to="/shipments" 
                className={({ isActive }) => `sidebar-nav-link ${isActive ? 'active' : ''}`}
              >
                Shipments
              </NavLink>
            </li>
          </ul>
        </nav>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  )
}

export default Layout
