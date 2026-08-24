function InternList({ interns, onInternDeleted }) {

    function deleteIntern(id) {
        fetch(`http://localhost:5053/api/interns/${id}`, {
            method: "DELETE"
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Stajyer silinemedi.");
                }

                onInternDeleted();
            })
            .catch(error => console.error(error));
    }

    return (
        <div>
            <h3>Stajyer Listesi</h3>

            {interns.length === 0 ? (
                <p>Henüz stajyer yok.</p>
            ) : (
                interns.map(intern => (
    <div className="intern-card" key={intern.id}>
        <strong>{intern.name}</strong>
        {" - "}
        {intern.email}
        {" - "}
        {intern.department?.name}

        <button onClick={() => deleteIntern(intern.id)}>
            Sil
        </button>
    </div>
    ))
            )}
        </div>
    );
}

export default InternList;