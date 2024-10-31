document.addEventListener("DOMContentLoaded", function () {

    // Dữ liệu học viên
    const hocVienData = [
        { anh: "User.jpg", maHocVIen: "2001215669", tenHocVien: "Bùi Khánh Duy", email: "buikhanhduy13082003@gmail.com" },
    ]

    const thongTinHocVien = document.getElementById("thongTinHocVien");

    hocVienData.forEach(hocvien => {
        const row = document.createElement("tr");

        const subjectCell = document.createElement("td");
        subjectCell.textContent = hocvien.anh;
        row.appendChild(subjectCell);

        const timeCell = document.createElement("td");
        timeCell.textContent = hocvien.maHocVIen;
        row.appendChild(timeCell);

        const roomCell = document.createElement("td");
        roomCell.textContent = hocvien.tenHocVien;
        row.appendChild(roomCell);

        const emailCell = document.createElement("td");
        emailCell.textContent = hocvien.email;
        row.appendChild(emailCell);

        thongTinHocVien.appendChild(row);
    });
});