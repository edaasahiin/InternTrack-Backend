import { Link } from "react-router-dom";
import sankoLogo from "../assets/sanko-logo.png";

function HomePage() {
    return (
        <div>
            <div className="hero">
                <img
                    src={sankoLogo}
                    alt="SANKO Logo"
                    className="company-logo"
                />

                <div>
                    <h1>InternTrack</h1>

                    <p>
                        Stajyer, departman ve görev yönetimi için geliştirilen
                        takip sistemi.
                    </p>
                </div>
            </div>

            <div className="dashboard">
                <Link
                    to="/interns"
                    className="dashboard-card-link"
                >
                    <div className="dashboard-card">
                        <h3>Stajyerler</h3>
                        <p>
                            Stajyer ekleme, listeleme ve silme işlemleri.
                        </p>
                    </div>
                </Link>

                <Link
                    to="/tasks"
                    className="dashboard-card-link"
                >
                    <div className="dashboard-card">
                        <h3>Görevler</h3>
                        <p>
                            Görev oluşturma, durum güncelleme ve silme işlemleri.
                        </p>
                    </div>
                </Link>

                <Link
                    to="/departments"
                    className="dashboard-card-link"
                >
                    <div className="dashboard-card">
                        <h3>Departmanlar</h3>
                        <p>
                            Departman ekleme, listeleme ve silme işlemleri.
                        </p>
                    </div>
                </Link>
            </div>
        </div>
    );
}

export default HomePage;