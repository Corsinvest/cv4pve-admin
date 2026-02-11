distro=$(uname -s)
require_reboot=$(test -e /var/run/reboot-required && echo true || echo false)
messages=()
success=false
pkg_manager=""

if [ "$distro" = "Linux" ]; then
    if [ "$(command -v lsb_release)" ]; then
        distro=$(lsb_release -si)
    elif [ -f /etc/os-release ]; then
        distro=$(grep "^PRETTY_NAME=" /etc/os-release | cut -d"=" -f2 | tr -d "\"" | awk "{print \$1, \$2}")
    elif [ -f /etc/redhat-release ]; then
        distro=$(cat /etc/redhat-release | awk "{print \$1, \$2}")
    else
        distro="Unknown"
    fi
elif [ "$distro" = "FreeBSD" ]; then
    distro="FreeBSD"
elif [ "$distro" = "Darwin" ]; then
    distro="macOS"
else
    distro="Unknown"
fi

case "${distro}" in
    "archlinux"|"Arch Linux"|"arch"|"Arch"|"archarm")
        distro="Arch Linux"
        pkg_manager="pacman"
        ;;

    "openSUSE"|"openSUSE project"|"SUSE LINUX"|"SUSE")
        distro="openSUSE"
        pkg_manager="zypper"
        ;;

    alpine)
        distro="Alpine Linux"
        pkg_manager="apk"
        ;;

    "Debian"*)
        if [ -f /etc/crunchbang-lsb-release ] || [ -f /etc/lsb-release-crunchbang ]; then
            distro="CrunchBang"
        elif [ -f /etc/siduction-version ]; then
            distro="Siduction"
        elif [ -f /usr/bin/pveversion ]; then
            distro="Proxmox VE"
        elif [ -f /etc/os-release ]; then
            if grep -q -i "Raspbian" /etc/os-release ; then
                distro="Raspbian"
            elif grep -q -i "BlankOn" /etc/os-release ; then
                distro="BlankOn"
            elif grep -q -i "Quirinux" /etc/os-release ; then
                distro="Quirinux"
            else
                distro="Debian"
            fi
        else
            distro="Debian"
        fi

        pkg_manager="apt"
        ;;

    "Ubuntu")
        pkg_manager="apt"
        ;;

    "FreeBSD")
        pkg_manager="pkg"
        ;;

    *)
        messages+=("Unsupported distribution: $distro")
        ;;
esac

case "${pkg_manager}" in
    "apt")
        apt update && apt upgrade -y && apt autoremove -y  && success=true
        ;;

    "yum")
        yum update -y && success=true
        ;;

    "pacman")
        pacman -Syu --noconfirm && success=true
        ;;

    "apk")
        apk update && apk upgrade && success=true
        ;;

    "zypper")
        zypper refresh && zypper update -y && success=true
        ;;

    "pkg")
        pkg update && pkg upgrade -y && success=true
        ;;

    *)
        messages+=("No update command found.")
        ;;
esac

if $success; then
    messages+=("Update completed successfully.")
else
    messages+=("Update failed.")
fi

json_messages="["
for i in "${!messages[@]}"; do
    msg=$(echo "${messages[$i]}" | sed 's/"/\\"/g')
    json_messages+="\"$msg\""
    [ $i -lt $((${#messages[@]} - 1)) ] && json_messages+=","
done
json_messages+="]"

echo -n "{\"success\":$success,\"messages\":$json_messages}"
