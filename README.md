<div align="center">

# 🎨 DepArt | Fine Art Logistics
### "Delivering Masterpieces with Precision."

![C#](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/Framework-.NET%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-00599C?style=for-the-badge&logo=windows&logoColor=white)
![IDE](https://img.shields.io/badge/IDE-Visual%20Studio-5C2D91?style=for-the-badge&logo=visual-studio&logoColor=white)

</div>

---

![App Demo](./Images/demo.gif)

### 📺 [Click Here to Watch the Full Video Demo (1:35 min)](https://youtu.be/Zayb3Yz2wZY)

---

## 📖 About The Project
**DepArt** is a specialized logistics solution tailored for high-end Art Galleries.
Transporting art requires more than just speed; it requires care. We built a system that manages the delicate process of delivering valuable assets from the gallery to the collector.

The system utilizes a **smart algorithm** to match the optimal courier to the package based on **real-time distance** and **vehicle capability** (Car, Motorcycle, Bicycle, or Foot).

### 📸 Gallery (Screenshots)
| Manager Dashboard | Courier Profile |
|:---:|:---:|
| ![Manager View](./Images/manager_view.png) | ![Courier View](./Images/courier_view.png) |

---

## ⚙️ Installation & Configuration

> ⚠️ **Security Notice:** This project uses external services (LocationIQ & Gmail SMTP).
> For security reasons, the API keys are **not** included in the repository. Follow the steps below to configure your local environment.

### 1. Clone the Repository
```bash
git clone [https://github.com/Miri-User/DepArt.git](https://github.com/Miri-User/DepArt.git)
```

### 2. Configure the "Secrets"
We use a secure method to handle API keys and Passwords without exposing them to Git.

1.  Navigate to `BL` -> `Helpers`.
2.  Locate the file: `Secrets.Template.cs`.
3.  **Create a copy** of this file and rename it to `Secrets.cs`.
4.  Update the fields with your own credentials:

```csharp
internal static class Secrets
{
    // 1. Map & Distance Service (Get free token from LocationIQ.com)
    internal const string LocationIqApiKey = "YOUR_LOCATIONIQ_KEY_HERE";

    // 2. Email Notification Service (Gmail App Password)
    internal const string ServiceEmail = "your.project.email@gmail.com";
    internal const string ServicePassword = "xxxx xxxx xxxx xxxx"; 
}
```

### 3. Run the System
Set **PL** as your startup project and hit **F5**!

---

## 🔑 How to Log In

The system comes pre-loaded with data so you can start testing immediately.

### 👨‍💼 Manager Access (Headquarters)
* **User ID:** `123456782`
* **Password:** `Deafult1234$`

### 🚚 Courier Access (Field)
To test the courier interface:
1.  Log in as a **Manager** first.
2.  Go to the **"Couriers"** tab and click **Add Courier**.
3.  Create a new courier profile.
4.  Log in using the **ID** and **Password** you just created.

---

## ✨ Key Features & Highlights

### 🕹️ The "God Mode" Simulator
Experience the system in action! The simulator runs on a separate thread (**Multi-Threading**), creating a lifelike environment where:
* Time moves faster (simulated clock).
* Orders are dispatched automatically based on courier availability.
* **Vehicle Logic:** The system calculates delivery times based on the courier's transport mode (Car vs. Bicycle).

### 📧 Real-Time Email Notifications
Unlike standard apps, DepArt integrates with **SMTP servers**.
The system sends automatic email confirmations when a parcel is delivered or when a critical alert is triggered.

### 🛡️ Robust Architecture
* **3-Tier Layered Architecture:** Strict separation between UI (PL), Logic (BL), and Data (DAL).
* **MVVM & Commands:** A responsive UI that reacts to data changes instantly.
* **Safe-Delete Logic:** You cannot delete a courier who is currently carrying a Mona Lisa! 🖼️
* **Smart Grouping:** Advanced filtering of orders by status and area using `CollectionViewSource`.

---

## 👩‍💻 Developed By

<div align="center">

**Miri**
<br>
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/miri-yaakobi-868790388/) [![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/MiriYaakobi)

<br>

**Odelia**
<br>
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/odelyagil/) [![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/OdelyaGil)

</div>

---
<div align="center">
Developed as a final project for Windows Application Programming Course, JCT.
</div>