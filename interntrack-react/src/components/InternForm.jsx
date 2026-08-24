import { useEffect, useState } from "react";

function InternForm({ onInternAdded }) {
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [departmentId, setDepartmentId] = useState("");
    const [departments, setDepartments] = useState([]);

    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        fetch("http://localhost:5053/api/departments")
            .then(response => response.json())
            .then(data => setDepartments(data))
            .catch(() => {
                setIsError(true);
                setMessage("Departmanlar yüklenemedi.");
            });
    }, []);

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setIsError(false);
        setIsSubmitting(true);

        const newIntern = {
            name,
            email,
            departmentId: Number(departmentId)
        };

        try {
            const response = await fetch(
                "http://localhost:5053/api/interns",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(newIntern)
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
                        setMessage("Girilen bilgiler geçersiz.");
                    }
                } else {
                    setMessage("Stajyer eklenemedi.");
                }

                return;
            }

            setIsError(false);
            setMessage(data.message || "Stajyer başarıyla eklendi.");

            setName("");
            setEmail("");
            setDepartmentId("");

            onInternAdded();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div>
            <h3>Stajyer Ekle</h3>

            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Ad"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />

                <input
                    type="email"
                    placeholder="Email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                    required
                />

                <select
                    value={departmentId}
                    onChange={e => setDepartmentId(e.target.value)}
                    required
                >
                    <option value="">Departman Seç</option>

                    {departments.map(department => (
                        <option
                            key={department.id}
                            value={department.id}
                        >
                            {department.name}
                        </option>
                    ))}
                </select>

                <button
                    type="submit"
                    disabled={isSubmitting}
                >
                    {isSubmitting ? "Ekleniyor..." : "Stajyer Ekle"}
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
        </div>
    );
}

export default InternForm;