import { useEffect, useState } from "react";

function DepartmentsPage() {
    const [departments, setDepartments] = useState([]);
    const [name, setName] = useState("");

    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    async function loadDepartments() {
        try {
            const response = await fetch(
                "http://localhost:5053/api/departments"
            );

            if (!response.ok) {
                throw new Error("Departmanlar yüklenemedi.");
            }

            const data = await response.json();
            setDepartments(data);
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Departmanlar yüklenemedi.");
        }
    }

    useEffect(() => {
        loadDepartments();
    }, []);

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setIsError(false);
        setIsSubmitting(true);

        const newDepartment = {
            name
        };

        try {
            const response = await fetch(
                "http://localhost:5053/api/departments",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(newDepartment)
                }
            );

            const data = await response.json();

            if (!response.ok) {
                setIsError(true);

                if (data.message) {
                    setMessage(data.message);
                } else if (data.errors) {
                    const firstError = Object.values(data.errors)[0];

                    if (Array.isArray(firstError)) {
                        setMessage(firstError[0]);
                    } else {
                        setMessage("Girilen departman bilgisi geçersiz.");
                    }
                } else {
                    setMessage("Departman eklenemedi.");
                }

                return;
            }

            setIsError(false);
            setMessage(data.message || "Departman oluşturuldu.");

            setName("");

            await loadDepartments();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
        } finally {
            setIsSubmitting(false);
        }
    }

    async function deleteDepartment(id) {
        setMessage("");
        setIsError(false);

        try {
            const response = await fetch(
                `http://localhost:5053/api/departments/${id}`,
                {
                    method: "DELETE"
                }
            );

            if (!response.ok) {
                let data = null;

                try {
                    data = await response.json();
                } catch {
                    data = null;
                }

                setIsError(true);
                setMessage(
                    data?.message || "Departman silinemedi."
                );

                return;
            }

            setIsError(false);
            setMessage("Departman başarıyla silindi.");

            await loadDepartments();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
        }
    }

    return (
        <div>
            <h2>Departmanlar</h2>

            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Departman Adı"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />

                <button
                    type="submit"
                    disabled={isSubmitting}
                >
                    {isSubmitting ? "Ekleniyor..." : "Departman Ekle"}
                </button>
            </form>

            {message && (
                <p
                    style={{
                        marginTop: "10px",
                        fontWeight: "bold"
                    }}
                >
                    {isError ? "❌ " : "✅ "}
                    {message}
                </p>
            )}

            <h3>Departman Listesi</h3>

            {departments.map(department => (
                <div
                    className="department-card"
                    key={department.id}
                >
                    {department.name}

                    <button
                        onClick={() =>
                            deleteDepartment(department.id)
                        }
                    >
                        Sil
                    </button>
                </div>
            ))}
        </div>
    );
}

export default DepartmentsPage;