import { useEffect, useState } from "react";

function InternForm({ onInternAdded }) {
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [departmentId, setDepartmentId] = useState("");
    const [departments, setDepartments] = useState([]);

    useEffect(() => {
        fetch("http://localhost:5053/api/departments")
            .then(response => response.json())
            .then(data => setDepartments(data));
    }, []);

    function handleSubmit(event) {
        event.preventDefault();

        const newIntern = {
            name,
            email,
            departmentId: Number(departmentId)
        };

        fetch("http://localhost:5053/api/interns", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newIntern)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Stajyer eklenemedi.");
                }

                return response.text();
            })
            .then(() => {
                setName("");
                setEmail("");
                setDepartmentId("");

                onInternAdded();
            })
            .catch(error => console.error(error));
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

                <button type="submit">
                    Stajyer Ekle
                </button>
            </form>
        </div>
    );
}

export default InternForm;